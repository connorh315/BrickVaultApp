using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;
using System;
using System.IO;

namespace BrickVaultApp;

public class FileInput : TemplatedControl
{
    private string _value = "";

    public string Value
    {
        get => _value;
        set
        {
            if (value == _value) return;

            SetAndRaise(ValueProperty, ref _value, value);
        }
    }

    public static readonly DirectProperty<FileInput, string> ValueProperty =
        AvaloniaProperty.RegisterDirect<FileInput, string>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public enum FileInputMode
    {
        OpenFile,
        SaveFile,
        OpenFolder
    }

    public static readonly StyledProperty<FileInputMode> ModeProperty =
    AvaloniaProperty.Register<FileInput, FileInputMode>(
        nameof(Mode),
        defaultValue: FileInputMode.OpenFile);

    public FileInputMode Mode
    {
        get => GetValue(ModeProperty);
        set => SetValue(ModeProperty, value);
    }

    public static readonly StyledProperty<string?> ExtensionProperty =
    AvaloniaProperty.Register<FileInput, string?>(
        nameof(Extension));

    public string? Extension
    {
        get => GetValue(ExtensionProperty);
        set => SetValue(ExtensionProperty, value);
    }

    private string? NormalizedExtension =>
    string.IsNullOrWhiteSpace(Extension)
        ? null
        : Extension.Trim().TrimStart('.').ToLowerInvariant();

    private FilePickerFileType[]? BuildFileTypeFilter()
    {
        if (NormalizedExtension is null)
            return null;

        var ext = NormalizedExtension;

        return new[]
        {
        new FilePickerFileType($"{ext.ToUpperInvariant()} file (*.{ext})")
            {
                Patterns = new[] { $"*.{ext}" }
            }
        };
    }

    private string CleanupPath(string path)
    {
        return path.Replace("/", "\\").Replace("%20", " ");
    }

    private async void Open_Filesystem_Picker(object? sender, RoutedEventArgs e)
    {
        if (this.GetVisualRoot() is not Window window)
            return;

        var storage = window.StorageProvider;
        if (storage is null)
            return;

        string startPath =
            !string.IsNullOrWhiteSpace(Value) &&
            Path.IsPathRooted(Value)
                ? (Directory.Exists(Value)
                    ? Value
                    : Path.GetDirectoryName(Value))
                : Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);

        var startLocation = await window.StorageProvider.TryGetFolderFromPathAsync(startPath);

        switch (Mode)
        {
            case FileInputMode.OpenFile:
                {
                    var files = await storage.OpenFilePickerAsync(new FilePickerOpenOptions
                    {
                        Title = "Open File",
                        AllowMultiple = false,
                        FileTypeFilter = BuildFileTypeFilter(),
                        SuggestedStartLocation = startLocation
                    });

                    if (files.Count > 0)
                        Value = CleanupPath(files[0].Path.AbsolutePath);
                    break;
                }

            case FileInputMode.SaveFile:
                {
                    var file = await storage.SaveFilePickerAsync(new FilePickerSaveOptions
                    {
                        Title = "Save File",
                        FileTypeChoices = BuildFileTypeFilter(),
                        SuggestedStartLocation = startLocation
                    });

                    if (file?.Path is { } path)
                        Value = CleanupPath(path.AbsolutePath);
                    break;
                }

            case FileInputMode.OpenFolder:
                {
                    var folders = await storage.OpenFolderPickerAsync(new FolderPickerOpenOptions
                    {
                        AllowMultiple = false,
                        SuggestedStartLocation = startLocation
                    });

                    if (folders.Count > 0)
                        Value = CleanupPath(
                            folders[0].Path.IsAbsoluteUri
                                ? folders[0].Path.AbsolutePath
                                : folders[0].Path.OriginalString);
                    break;
                }
        }
    }


    private void TextBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not TextBox textbox) return;

        // Basically, this forces the input to be valid, either by using the last appropriate value, or best case scenario it just updates back to itself again.
        string original = Value;
        Value = "00000";
        Value = original;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var tb = e.NameScope.Find<TextBox>("PART_Textbox");
        tb.LostFocus += TextBox_LostFocus;

        var button = e.NameScope.Find<Button>("PART_Button");
        button.Click += Open_Filesystem_Picker;
    }
}