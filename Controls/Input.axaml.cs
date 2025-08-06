using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Data;
using System;

namespace BrickVaultApp;

public class Input : TemplatedControl
{

    /// <summary>
    /// InputLabel StyledProperty definition
    /// </summary>
    public static readonly StyledProperty<string> InputLabelProperty =
        AvaloniaProperty.Register<Input, string>(nameof(InputLabel), "Input:");

    /// <summary>
    /// Gets or sets the InputLabel property. This StyledProperty 
    /// indicates ....
    /// </summary>
    public string InputLabel
    {
        get => this.GetValue(InputLabelProperty);
        set => SetValue(InputLabelProperty, value);
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

            if (NumericValue && !string.IsNullOrEmpty(value))
            {
                if (FloatValue)
                {
                    // Allow float input
                    if (!float.TryParse(value, out _))
                        throw new DataValidationException("Numeric value only");
                }
                else
                {
                    // Only allow integer input
                    if (!int.TryParse(value, out _))
                        throw new DataValidationException("Integer input only");
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
    }
}