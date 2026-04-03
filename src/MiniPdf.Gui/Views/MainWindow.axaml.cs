using System;
using System.Diagnostics;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MiniPdf.Gui.ViewModels;

namespace MiniPdf.Gui.Views;

public partial class MainWindow : Window
{
    private MainWindowViewModel ViewModel => (MainWindowViewModel)DataContext!;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        AddHandler(DragDrop.DropEvent, OnDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
    }

    private void OnDragOver(object? sender, DragEventArgs e)
    {
        e.DragEffects = e.Data.Contains(DataFormats.Files)
            ? DragDropEffects.Copy
            : DragDropEffects.None;
    }

    private void OnDrop(object? sender, DragEventArgs e)
    {
        if (e.Data.GetFiles() is { } files)
        {
            var paths = files.Select(f => f.TryGetLocalPath()).Where(p => p is not null).ToArray();
            ViewModel.AddFiles(paths!);
        }
    }

    private async void OnOpenFilesClick(object? sender, RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Office files",
            AllowMultiple = true,
            FileTypeFilter = new[]
            {
                new FilePickerFileType("Office Documents")
                {
                    Patterns = new[] { "*.docx", "*.xlsx" }
                }
            }
        });

        if (files.Count > 0)
        {
            var paths = files.Select(f => f.TryGetLocalPath()).Where(p => p is not null).ToArray();
            ViewModel.AddFiles(paths!);
        }
    }

    private async void OnOpenFolderClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select folder containing Office files",
            AllowMultiple = false
        });

        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path)
        {
            ViewModel.AddFiles(path);
        }
    }

    private void OnOpenOutputClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: FileItem { OutputPath: { } outputPath } })
        {
            var dir = System.IO.Path.GetDirectoryName(outputPath);
            if (dir != null && System.IO.Directory.Exists(dir))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = dir,
                    UseShellExecute = true
                });
            }
        }
    }

    private async void OnBrowseFontDirClick(object? sender, RoutedEventArgs e)
    {
        var folders = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select font directory",
            AllowMultiple = false
        });

        if (folders.Count > 0 && folders[0].TryGetLocalPath() is { } path)
        {
            ViewModel.FontDirectory = path;
        }
    }
}
