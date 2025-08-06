using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BrickVault;
using BrickVaultApp.ViewModels;
using System.ComponentModel;

namespace BrickVaultApp;

public partial class BuildProgressWindow : Window
{
    public BuildProgressWindow()
    {
        InitializeComponent();
    }

    public BuildProgressWindow(BuildProgress progress)
    {
        InitializeComponent();
        DataContext = new BuildProgressViewModel(progress);
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private async void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BuildProgressViewModel vm) return;

        if (vm.ButtonString == "Cancel")
        {
            vm.Progress.Cancel();
        }
        else
        {
            Close();
        }
    }
}