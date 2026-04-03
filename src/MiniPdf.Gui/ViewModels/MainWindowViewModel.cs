using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MiniSoftware;

namespace MiniPdf.Gui.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private string _statusText = "Ready";

    [ObservableProperty]
    private bool _isConverting;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private string _fontDirectory = string.Empty;

    public ObservableCollection<FileItem> Files { get; } = new();

    public MainWindowViewModel()
    {
        Files.CollectionChanged += (_, _) => ConvertAllCommand.NotifyCanExecuteChanged();
    }

    public void AddFiles(params string[] paths)
    {
        foreach (var path in paths)
        {
            if (Directory.Exists(path))
            {
                var files = Directory.EnumerateFiles(path, "*.*", SearchOption.TopDirectoryOnly)
                    .Where(f => IsSupportedFile(f));
                foreach (var file in files)
                    AddFileIfNotExists(file);
            }
            else if (File.Exists(path) && IsSupportedFile(path))
            {
                AddFileIfNotExists(path);
            }
        }
    }

    private void AddFileIfNotExists(string filePath)
    {
        if (Files.All(f => !string.Equals(f.InputPath, filePath, StringComparison.OrdinalIgnoreCase)))
        {
            Files.Add(new FileItem(filePath));
        }
    }

    private static bool IsSupportedFile(string path)
    {
        var ext = Path.GetExtension(path);
        return ext.Equals(".docx", StringComparison.OrdinalIgnoreCase)
            || ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase);
    }

    [RelayCommand]
    private void RemoveFile(FileItem item)
    {
        Files.Remove(item);
    }

    [RelayCommand]
    private void ClearFiles()
    {
        Files.Clear();
        StatusText = "Ready";
        Progress = 0;
    }

    [RelayCommand(CanExecute = nameof(CanConvert))]
    private async Task ConvertAllAsync()
    {
        IsConverting = true;
        Progress = 0;

        RegisterFonts();

        var total = Files.Count;
        var completed = 0;

        foreach (var file in Files)
        {
            if (file.Status == ConvertStatus.Done)
            {
                completed++;
                continue;
            }

            file.Status = ConvertStatus.Converting;
            StatusText = $"Converting {file.FileName}...";

            try
            {
                var outputPath = Path.ChangeExtension(file.InputPath, ".pdf");
                await Task.Run(() => MiniSoftware.MiniPdf.ConvertToPdf(file.InputPath, outputPath));
                file.OutputPath = outputPath;
                file.Status = ConvertStatus.Done;
            }
            catch (Exception ex)
            {
                file.Status = ConvertStatus.Error;
                file.ErrorMessage = ex.Message;
            }

            completed++;
            Progress = (double)completed / total * 100;
        }

        IsConverting = false;
        var errors = Files.Count(f => f.Status == ConvertStatus.Error);
        StatusText = errors > 0
            ? $"Completed with {errors} error(s)"
            : $"All {total} file(s) converted successfully";
    }

    private bool CanConvert() => Files.Count > 0 && !IsConverting;

    partial void OnIsConvertingChanged(bool value) => ConvertAllCommand.NotifyCanExecuteChanged();

    private void RegisterFonts()
    {
        if (string.IsNullOrWhiteSpace(FontDirectory) || !Directory.Exists(FontDirectory))
            return;

        foreach (var fontFile in Directory.EnumerateFiles(FontDirectory, "*.*")
            .Where(f => f.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase)
                     || f.EndsWith(".ttc", StringComparison.OrdinalIgnoreCase)))
        {
            var name = Path.GetFileNameWithoutExtension(fontFile);
            MiniSoftware.MiniPdf.RegisterFont(name, File.ReadAllBytes(fontFile));
        }
    }
}

public partial class FileItem : ObservableObject
{
    public string InputPath { get; }
    public string FileName => Path.GetFileName(InputPath);
    public string FileType => Path.GetExtension(InputPath).TrimStart('.').ToUpperInvariant();

    [ObservableProperty]
    private ConvertStatus _status = ConvertStatus.Pending;

    [ObservableProperty]
    private string? _outputPath;

    [ObservableProperty]
    private string? _errorMessage;

    public FileItem(string inputPath) => InputPath = inputPath;
}

public enum ConvertStatus
{
    Pending,
    Converting,
    Done,
    Error
}
