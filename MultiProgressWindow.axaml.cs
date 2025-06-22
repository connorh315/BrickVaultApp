using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BrickVault.Types;
using BrickVaultApp.ViewModels;
using System.Collections;
using System.Collections.Generic;

namespace BrickVaultApp;

public partial class MultiProgressWindow : Window
{
    public MultiProgressWindow() { }
    public MultiProgressWindow(IEnumerable<DATFile> files, string outputLocation)
    {
        InitializeComponent();
        DataContext = new MultiProgressWindowViewModel(files, outputLocation);
        this.SizeToContent = SizeToContent.WidthAndHeight;

        this.Closing += (s, e) =>
        {
            if (DataContext is not MultiProgressWindowViewModel vm)
                return;

            vm.ButtonPress();
        };
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is MultiProgressWindowViewModel vm)
        {
            if (vm.ButtonPress()) Close();
        }
    }
}