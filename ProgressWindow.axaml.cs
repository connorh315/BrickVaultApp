using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Platform;
using Avalonia.Threading;
using BrickVault;
using BrickVaultApp.ViewModels;
using System.Diagnostics;

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