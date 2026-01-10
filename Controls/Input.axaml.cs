using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System;

namespace BrickVaultApp;

[PseudoClasses(":error")]
public class Input : TemplatedControl
{

    /// <summary>
    /// InputLabel StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> LabelProperty =
        AvaloniaProperty.Register<Input, string>(nameof(Label), null);

    /// <summary>
    /// Gets or sets the InputLabel property. This StyledProperty 
    /// indicates ....
    /// </summary>
    public string Label
    {
        get => this.GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly StyledProperty<bool> RequiredProperty =
    AvaloniaProperty.Register<Input, bool>(
        nameof(Required),
        defaultValue: false);

    public bool Required
    {
        get => GetValue(RequiredProperty);
        set => SetValue(RequiredProperty, value);
    }

    public static readonly StyledProperty<string?> WatermarkProperty =
    AvaloniaProperty.Register<Input, string?>(
        nameof(Watermark));

    public string? Watermark
    {
        get => GetValue(WatermarkProperty);
        set => SetValue(WatermarkProperty, value);
    }


    private string _value = "";

    public static readonly DirectProperty<Input, string> ValueProperty =
        AvaloniaProperty.RegisterDirect<Input, string>(
            nameof(Value),
            o => o.Value,
            (o, v) => o.Value = v,
            defaultBindingMode: BindingMode.TwoWay, enableDataValidation: true);

    public string Value
    {
        get => _value;
        set
        {
            if (value == _value) return;

            SetError(value == string.Empty && Required);

            if (NumericValue && !string.IsNullOrEmpty(value))
            {
                if (FloatValue)
                {
                    // Allow float input
                    if (!float.TryParse(value, out _))
                        SetError(true);
                }
                else
                {
                    // Only allow integer input
                    if (!int.TryParse(value, out _))
                        SetError(true);
                }
            }

            SetAndRaise(ValueProperty, ref _value, value);
        }
    }

    /// <summary>
    /// Gets or sets the NumericValue property. This StyledProperty 
    /// indicates ....
    /// </summary>
    public bool NumericValue
    {
        get => this.GetValue(NumericValueProperty);
        set => SetValue(NumericValueProperty, value);
    }

    /// <summary>
    /// NumericValue StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<bool> NumericValueProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(NumericValue), false);


    /// <summary>
    /// FloatValue StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<bool> FloatValueProperty =
        AvaloniaProperty.Register<Input, bool>(nameof(FloatValue), false);

    /// <summary>
    /// Gets or sets the FloatValue property. This StyledProperty
    /// indicates ....
    /// </summary>
    public bool FloatValue
    {
        get => this.GetValue(FloatValueProperty);
        set => SetValue(FloatValueProperty, value);
    }



    /// <summary>
    /// This is so so so stupid that I have to do this. Why is there nothing better than this in Avalonia anyway????
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void TextBox_LostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is not TextBox textbox) return;

        if (string.IsNullOrEmpty(Value))
            SetError(true);

    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        var tb = e.NameScope.Find<TextBox>("PART_Textbox");
        tb.LostFocus += TextBox_LostFocus;
    }

    private bool _hasError = false;

    private void SetError(bool error)
    {
        if (!Required) return;
        _hasError = error;
        PseudoClasses.Set(":error", error);
    }
}