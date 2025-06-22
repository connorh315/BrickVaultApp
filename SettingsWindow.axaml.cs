using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
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
    }

    private void SaveSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsWindowViewModel vm)
        {
            vm.SaveSettings();
            
            Close();
        }
    }
}