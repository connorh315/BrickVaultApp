using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BrickVaultApp.Settings;
using BrickVaultApp.ViewModels;

namespace BrickVaultApp;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    public SettingsWindow(AppSettings settings)
    {
        InitializeComponent();
        DataContext = new SettingsWindowViewModel(settings);
        this.SizeToContent = SizeToContent.WidthAndHeight;

        this.Closing += (s, e) =>
        {
            if (DataContext is SettingsWindowViewModel vm)
            { // If the user tries to just close the window via the "X" then rebuild the app settings
                AppSettings.Settings = AppSettings.Load();
            }
        };
    }

    private void NewOpenWithEntry_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel vm)
        {
            vm.AddNewOpenWithEntry();
        }
    }

    private void SaveSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel vm)
        {
            if (!vm.Validate()) return;

            vm.SaveSettings();
            
            Close();
        }
    }
}