using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using BrickVaultApp.ViewModels;

namespace BrickVaultApp;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new AboutWindowViewModel();
        SizeToContent = SizeToContent.WidthAndHeight;
    }
}