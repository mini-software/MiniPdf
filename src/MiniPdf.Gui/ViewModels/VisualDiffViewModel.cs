using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MiniPdf.Gui.ViewModels;

public partial class VisualDiffViewModel : ObservableObject, IDisposable
{
    private readonly List<DiffCaseItem> _allCases = new();

    [ObservableProperty]
    private string _reportPath = string.Empty;

    [ObservableProperty]
    private string _searchText = string.Empty;

    [ObservableProperty]
    private string _statusText = "Open a classic or issue comparison report to begin";

    [ObservableProperty]
    private DiffCaseItem? _selectedCase;

    [ObservableProperty]
    private DiffPageItem? _selectedPage;

    [ObservableProperty]
    private Bitmap? _candidateImage;

    [ObservableProperty]
    private Bitmap? _referenceImage;

    [ObservableProperty]
    private Bitmap? _heatmapImage;

    [ObservableProperty]
    private double _zoom = 0.7;

    [ObservableProperty]
    private bool _showHeatmap;

    [ObservableProperty]
    private string _pageMetrics = string.Empty;

    public ObservableCollection<DiffCaseItem> Cases { get; } = new();

    public string ZoomText => $"{Zoom * 100:0}%";

    public bool HasHeatmap => HeatmapImage is not null;

    public double CandidateDisplayWidth => CandidateImage?.PixelSize.Width * Zoom ?? 0;

    public double ReferenceDisplayWidth => ReferenceImage?.PixelSize.Width * Zoom ?? 0;

    public double HeatmapDisplayWidth => HeatmapImage?.PixelSize.Width * Zoom ?? 0;

    public void LoadReport(string reportJsonPath)
    {
        if (!File.Exists(reportJsonPath))
            throw new FileNotFoundException("Comparison report was not found.", reportJsonPath);

        using var stream = File.OpenRead(reportJsonPath);
        using var document = JsonDocument.Parse(stream);
        if (document.RootElement.ValueKind != JsonValueKind.Array)
            throw new InvalidDataException("comparison_report.json must contain a JSON array.");

        var reportDirectory = Path.GetDirectoryName(Path.GetFullPath(reportJsonPath))!;
        var loadedCases = new List<DiffCaseItem>();

        foreach (var result in document.RootElement.EnumerateArray())
        {
            if (result.ValueKind != JsonValueKind.Object)
                continue;

            var name = GetString(result, "display_name") ?? GetString(result, "name") ?? "Unnamed case";
            var rawName = GetString(result, "name") ?? name;
            var pages = ReadPages(result, reportDirectory);
            loadedCases.Add(new DiffCaseItem(
                name,
                rawName,
                GetDouble(result, "overall_score"),
                GetDouble(result, "text_similarity"),
                GetDouble(result, "visual_avg"),
                GetInt(result, "minipdf_pages"),
                GetInt(result, "reference_pages"),
                BuildMetadata(result),
                pages));
        }

        DisposeImages();
        _allCases.Clear();
        _allCases.AddRange(loadedCases.OrderBy(item => item.Name, StringComparer.OrdinalIgnoreCase));
        ReportPath = Path.GetFullPath(reportJsonPath);
        SearchText = string.Empty;
        ApplyFilter();
        SelectedCase = Cases.FirstOrDefault();

        var scores = _allCases.Where(item => item.Score.HasValue).Select(item => item.Score!.Value).ToArray();
        var average = scores.Length == 0 ? "n/a" : $"{scores.Average():P1}";
        StatusText = $"{_allCases.Count} cases, {_allCases.Sum(item => item.Pages.Count)} rendered pages, average {average}";
    }

    public bool TryLoadKnownReport(string relativePath)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, relativePath);
            if (File.Exists(candidate))
            {
                LoadReport(candidate);
                return true;
            }
            directory = directory.Parent;
        }

        StatusText = $"Report not found: {relativePath}. Run the benchmark or browse to comparison_report.json.";
        return false;
    }

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnSelectedCaseChanged(DiffCaseItem? value)
    {
        SelectedPage = value?.Pages.FirstOrDefault();
    }

    partial void OnSelectedPageChanged(DiffPageItem? value)
    {
        DisposeImages();
        CandidateImage = LoadBitmap(value?.CandidatePath);
        ReferenceImage = LoadBitmap(value?.ReferencePath);
        HeatmapImage = LoadBitmap(value?.HeatmapPath);
        ShowHeatmap = false;
        PageMetrics = value?.Metrics ?? string.Empty;
        OnPropertyChanged(nameof(HasHeatmap));
    }

    partial void OnZoomChanged(double value)
    {
        OnPropertyChanged(nameof(ZoomText));
        OnPropertyChanged(nameof(CandidateDisplayWidth));
        OnPropertyChanged(nameof(ReferenceDisplayWidth));
        OnPropertyChanged(nameof(HeatmapDisplayWidth));
    }

    partial void OnCandidateImageChanged(Bitmap? value) => OnPropertyChanged(nameof(CandidateDisplayWidth));

    partial void OnReferenceImageChanged(Bitmap? value) => OnPropertyChanged(nameof(ReferenceDisplayWidth));

    partial void OnHeatmapImageChanged(Bitmap? value)
    {
        OnPropertyChanged(nameof(HasHeatmap));
        OnPropertyChanged(nameof(HeatmapDisplayWidth));
    }

    [RelayCommand]
    private void ZoomIn() => Zoom = Math.Min(2.5, Math.Round(Zoom + 0.1, 2));

    [RelayCommand]
    private void ZoomOut() => Zoom = Math.Max(0.2, Math.Round(Zoom - 0.1, 2));

    [RelayCommand]
    private void ResetZoom() => Zoom = 0.7;

    [RelayCommand]
    private void ToggleHeatmap()
    {
        if (HasHeatmap)
            ShowHeatmap = !ShowHeatmap;
    }

    private void ApplyFilter()
    {
        var selected = SelectedCase;
        var query = SearchText.Trim();
        var filtered = string.IsNullOrWhiteSpace(query)
            ? _allCases
            : _allCases.Where(item => item.SearchText.Contains(query, StringComparison.OrdinalIgnoreCase)).ToList();

        Cases.Clear();
        foreach (var item in filtered)
            Cases.Add(item);

        SelectedCase = selected is not null && Cases.Contains(selected)
            ? selected
            : Cases.FirstOrDefault();
    }

    private static IReadOnlyList<DiffPageItem> ReadPages(JsonElement result, string reportDirectory)
    {
        if (!result.TryGetProperty("diff_images", out var images) || images.ValueKind != JsonValueKind.Array)
            return Array.Empty<DiffPageItem>();

        var visualScores = new List<double?>();
        if (result.TryGetProperty("visual_scores", out var scores) && scores.ValueKind == JsonValueKind.Array)
        {
            visualScores.AddRange(scores.EnumerateArray().Select(element => element.TryGetDouble(out var score) ? score : (double?)null));
        }

        var pages = new List<DiffPageItem>();
        var index = 0;
        foreach (var image in images.EnumerateArray())
        {
            var pageNumber = GetInt(image, "page") ?? index + 1;
            var score = index < visualScores.Count ? visualScores[index] : null;
            var candidatePath = ResolveImagePath(reportDirectory, GetString(image, "minipdf_img"));
            var referencePath = ResolveImagePath(reportDirectory, GetString(image, "reference_img"));
            var heatmapPath = ResolveImagePath(reportDirectory, GetString(image, "heatmap_img"));
            pages.Add(new DiffPageItem(pageNumber, score, candidatePath, referencePath, heatmapPath, BuildPageMetrics(image)));
            index++;
        }
        return pages;
    }

    private static string? ResolveImagePath(string reportDirectory, string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return null;
        var path = Path.Combine(reportDirectory, "images", fileName);
        return File.Exists(path) ? path : null;
    }

    private static string BuildMetadata(JsonElement result)
    {
        var values = new[]
        {
            ("format", GetString(result, "format")),
            ("case", GetString(result, "case_id")),
            ("scope", GetString(result, "report_scope")),
        };
        return string.Join(" | ", values.Where(item => !string.IsNullOrWhiteSpace(item.Item2)).Select(item => $"{item.Item1}: {item.Item2}"));
    }

    private static string BuildPageMetrics(JsonElement image)
    {
        if (!image.TryGetProperty("heatmap_metrics", out var metrics) || metrics.ValueKind != JsonValueKind.Object)
            return string.Empty;

        var changedFraction = GetDouble(metrics, "changed_fraction");
        var changedPixels = GetInt(metrics, "changed_pixels");
        var rmse = GetDouble(metrics, "rmse_rgb");
        return $"Changed: {(changedFraction.HasValue ? changedFraction.Value.ToString("P2") : "n/a")} ({changedPixels?.ToString("N0") ?? "n/a"} px) | RMSE: {rmse?.ToString("0.####") ?? "n/a"}";
    }

    private static string? GetString(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static double? GetDouble(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetDouble(out var value)
            ? value
            : null;

    private static int? GetInt(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
            ? value
            : null;

    private static Bitmap? LoadBitmap(string? path)
    {
        if (path is null || !File.Exists(path))
            return null;
        using var stream = File.OpenRead(path);
        return new Bitmap(stream);
    }

    private void DisposeImages()
    {
        CandidateImage?.Dispose();
        ReferenceImage?.Dispose();
        HeatmapImage?.Dispose();
        CandidateImage = null;
        ReferenceImage = null;
        HeatmapImage = null;
    }

    public void Dispose() => DisposeImages();
}

public sealed record DiffCaseItem(
    string Name,
    string RawName,
    double? Score,
    double? TextSimilarity,
    double? VisualAverage,
    int? MiniPdfPages,
    int? ReferencePages,
    string Metadata,
    IReadOnlyList<DiffPageItem> Pages)
{
    public string ScoreText => Score.HasValue ? Score.Value.ToString("P1") : "n/a";
    public string VisualText => VisualAverage.HasValue ? VisualAverage.Value.ToString("P1") : "n/a";
    public string PageCountText => $"{MiniPdfPages?.ToString() ?? "?"} / {ReferencePages?.ToString() ?? "?"}";
    public string SearchText => $"{Name} {RawName} {Metadata}";
}

public sealed record DiffPageItem(
    int PageNumber,
    double? VisualScore,
    string? CandidatePath,
    string? ReferencePath,
    string? HeatmapPath,
    string Metrics)
{
    public string Title => $"Page {PageNumber}";
    public string ScoreText => VisualScore.HasValue ? VisualScore.Value.ToString("P1") : "n/a";
}