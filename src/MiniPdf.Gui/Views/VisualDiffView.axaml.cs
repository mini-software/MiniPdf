using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MiniPdf.Gui.ViewModels;

namespace MiniPdf.Gui.Views;

public partial class VisualDiffView : UserControl
{
    private bool _synchronizingScroll;

    private VisualDiffViewModel ViewModel => (VisualDiffViewModel)DataContext!;

    public VisualDiffView()
    {
        InitializeComponent();
    }

    private async void OnOpenReportClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel is null)
            return;

        var files = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open MiniPdf comparison report",
            AllowMultiple = false,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("MiniPdf comparison report")
                {
                    Patterns = new[] { "comparison_report.json", "*.json" }
                }
            }
        });

        if (files.Count > 0 && files[0].TryGetLocalPath() is { } path)
        {
            try
            {
                ViewModel.LoadReport(path);
            }
            catch (Exception ex)
            {
                ViewModel.StatusText = ex.Message;
            }
        }
    }

    private void OnClassicXlsxClick(object? sender, RoutedEventArgs e) =>
        ViewModel.TryLoadKnownReport(PathFor("tests", "MiniPdf.Benchmark", "reports", "comparison_report.json"));

    private void OnClassicDocxClick(object? sender, RoutedEventArgs e) =>
        ViewModel.TryLoadKnownReport(PathFor("tests", "MiniPdf.Benchmark", "reports_docx", "comparison_report.json"));

    private void OnIssueXlsxClick(object? sender, RoutedEventArgs e) =>
        ViewModel.TryLoadKnownReport(PathFor("tests", "Issue_Files", "reports_xlsx", "comparison_report.json"));

    private void OnIssueDocxClick(object? sender, RoutedEventArgs e) =>
        ViewModel.TryLoadKnownReport(PathFor("tests", "Issue_Files", "reports_docx", "comparison_report.json"));

    private static string PathFor(params string[] parts) => System.IO.Path.Combine(parts);

    private void OnRevealReportClick(object? sender, RoutedEventArgs e) =>
        RevealInFileExplorer(ViewModel.ReportPath);

    private void OnRevealCandidateClick(object? sender, RoutedEventArgs e) =>
        RevealInFileExplorer(ViewModel.SelectedPage?.CandidatePath);

    private void OnRevealReferenceClick(object? sender, RoutedEventArgs e) =>
        RevealInFileExplorer(ViewModel.SelectedPage?.ReferencePath);

    private void OnRevealHeatmapClick(object? sender, RoutedEventArgs e) =>
        RevealInFileExplorer(ViewModel.SelectedPage?.HeatmapPath);

    private void RevealInFileExplorer(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        try
        {
            var fullPath = Path.GetFullPath(path);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var arguments = File.Exists(fullPath)
                    ? $"/select,\"{fullPath}\""
                    : $"\"{Path.GetDirectoryName(fullPath) ?? fullPath}\"";
                Process.Start(new ProcessStartInfo("explorer.exe", arguments)
                {
                    UseShellExecute = true
                });
                return;
            }

            var directory = Directory.Exists(fullPath) ? fullPath : Path.GetDirectoryName(fullPath);
            if (directory is not null)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = directory,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            ViewModel.StatusText = $"Could not open File Explorer: {ex.Message}";
        }
    }

    private void OnCandidateScrollChanged(object? sender, ScrollChangedEventArgs e) =>
        SynchronizeScroll(CandidateScroll, ReferenceScroll);

    private void OnReferenceScrollChanged(object? sender, ScrollChangedEventArgs e) =>
        SynchronizeScroll(ReferenceScroll, CandidateScroll);

    private void SynchronizeScroll(ScrollViewer source, ScrollViewer target)
    {
        if (_synchronizingScroll)
            return;

        _synchronizingScroll = true;
        try
        {
            var horizontalRatio = source.Extent.Width > source.Viewport.Width
                ? source.Offset.X / (source.Extent.Width - source.Viewport.Width)
                : 0;
            var verticalRatio = source.Extent.Height > source.Viewport.Height
                ? source.Offset.Y / (source.Extent.Height - source.Viewport.Height)
                : 0;
            target.Offset = new Vector(
                horizontalRatio * Math.Max(0, target.Extent.Width - target.Viewport.Width),
                verticalRatio * Math.Max(0, target.Extent.Height - target.Viewport.Height));
        }
        finally
        {
            _synchronizingScroll = false;
        }
    }
}