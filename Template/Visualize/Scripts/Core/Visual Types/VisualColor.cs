﻿using Godot;
using RedotUtils;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualColor(VisualControlContext context)
    {
        Color initialColor = (Color)context.InitialValue;

        RColorPickerButton colorPickerButton = new(initialColor);
        colorPickerButton.OnColorChanged += color => context.ValueChanged(color);

        return new VisualControlInfo(new ColorPickerButtonControl(colorPickerButton));
    }
}

public class ColorPickerButtonControl(RColorPickerButton colorPickerButton) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value is Color color)
        {
            colorPickerButton.Control.Color = color;
        }
    }

    public Control Control => colorPickerButton.Control;

    public void SetEditable(bool editable)
    {
        colorPickerButton.Control.Disabled = !editable;
    }
}
