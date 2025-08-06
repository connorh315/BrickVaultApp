using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BrickVaultApp.ViewModels;

namespace BrickVaultApp;

public partial class BuildSettingsWindow : Window
{
    public BuildSettingsWindow()
    {
        InitializeComponent();
    }

    public BuildSettingsWindow(BuildArchiveViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private async void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BuildArchiveViewModel vm) return;

        vm.ShouldDelete = true; // Parent window should delete

        Close();
    }

    private async void SaveSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BuildArchiveViewModel vm) return;

        vm.CommitSettings = true; // Means that the window closed with the "Save settings" button, rather than by closing the window.

        Close();
    }
}