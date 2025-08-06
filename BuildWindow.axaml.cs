using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BrickVaultApp.ViewModels;
using System;

namespace BrickVaultApp;

public partial class BuildWindow : Window
{
    public BuildArchiveViewModel SelectedBuild;

    public BuildWindow()
    {
        DataContext = new BuildWindowViewModel();
        InitializeComponent();
        SizeToContent = SizeToContent.WidthAndHeight;
    }

    private async void NewArchive_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not BuildWindowViewModel vm) return;

        await vm.AddToBuildSettings(this);
    }

    private async void Build_Click(object? sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not BuildArchiveViewModel buildVM) return;

        SelectedBuild = buildVM;

        Close();
    }

    private async void Edit_Click(object? sender, RoutedEventArgs e)
    {
        if (((Button)sender).DataContext is not BuildArchiveViewModel buildVM) return;

        if (DataContext is not BuildWindowViewModel vm) return;

        await vm.OpenBuildSettings(this, buildVM);
    }
}