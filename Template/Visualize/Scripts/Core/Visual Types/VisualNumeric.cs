using Godot;
using GodotUtils;
using System;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualNumeric(Type type, VisualControlContext context)
    {
        SpinBox spinBox = CreateSpinBox(type);

        spinBox.Value = Convert.ToDouble(context.InitialValue);
        spinBox.ValueChanged += value =>
        {
            object convertedValue = Convert.ChangeType(value, type);
            context.ValueChanged(convertedValue);
        };

        return new VisualControlInfo(new NumericControl(spinBox));
    }
}

public class NumericControl(SpinBox spinBox) : IVisualControl
{
    public void SetValue(object value)
    {
        if (value != null)
        {
            try
            {
                spinBox.Value = Convert.ToDouble(value);
            }
            catch (InvalidCastException)
            {
                // Handle the case where the value cannot be converted to double
                PrintUtils.Warning($"Cannot convert value of type {value.GetType()} to double.");
            }
        }
    }

    public Control Control => spinBox;

    public void SetEditable(bool editable)
    {
        spinBox.Editable = editable;
    }
}
