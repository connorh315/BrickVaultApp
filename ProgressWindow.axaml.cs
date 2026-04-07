using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using BrickVault;
using BrickVaultApp.ViewModels;
using System;
using System.Diagnostics;
using System.IO;

namespace BrickVaultApp;

public partial class ProgressWindow : Window
{
    public ProgressWindow(ThreadedExtractionCtx extractionCtx) : this()
    {
        DataContext = new ProgressWindowViewModel(extractionCtx);

        this.Closing += (s, e) =>
        {
            if (DataContext is not ProgressWindowViewModel vm)
                return;

            vm.OnWindowClose();
        };

        this.KeyDown += ProgressWindow_KeyDown;
    }

    private void ProgressWindow_KeyDown(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (DataContext is not ProgressWindowViewModel vm)
            return;

        if (vm.HasComplete && (e.Key == Avalonia.Input.Key.Space || e.Key == Avalonia.Input.Key.Escape))
        {
            Close();
        }
    }

    private void NavigateToClick(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ProgressWindowViewModel vm)
            return;

        if (!vm.HasComplete) return;

        string cleanup = Path.GetFullPath(vm.NavigateLocation);

        var psi = new ProcessStartInfo
        {
            FileName = "explorer.exe",
            UseShellExecute = true
        };

        if (vm.ShouldSelect)
        {
            psi.Arguments = $"/select,\"{cleanup}\"";
        }
        else if (Directory.Exists(cleanup))
        {
            psi.Arguments = $"\"{cleanup}\"";
        }
        else if (File.Exists(cleanup))
        {
            psi.Arguments = $"/select,\"{cleanup}\"";
        }
        else
        {
            return;
        }

        Process.Start(psi);
    }

    private void OnInteractClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    public ProgressWindow()
    {
        InitializeComponent();

        this.SizeToContent = SizeToContent.Manual;
        Width = 300;
        Height = 150;
    }
}