using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Template;

public static partial class VisualControlTypes
{
    private static VisualControlInfo VisualClass(Type type, VisualControlContext context)
    {
        VBoxContainer vbox = new();

        if (context.InitialValue == null)
        {
            throw new Exception($"[Visualize] Contexts initial value was null for type '{type}'");
        }

        AddProperties(vbox, type, context, out List<IVisualControl> propertyControls, out PropertyInfo[] properties);
        AddFields(vbox, type, context, out List<IVisualControl> fieldControls, out FieldInfo[] fields);
        AddMethods(vbox, type, context);

        return new VisualControlInfo(new ClassControl(vbox, propertyControls, fieldControls, properties, fields));
    }

    private static void AddProperties(VBoxContainer vbox, Type type, VisualControlContext context, out List<IVisualControl> propertyControls, out PropertyInfo[] properties)
    {
        propertyControls = [];

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        properties = type.GetProperties(flags)
            // Exclude delegate types
            .Where(p => !(typeof(Delegate).IsAssignableFrom(p.PropertyType)))
            .ToArray();

        foreach (PropertyInfo property in properties)
        {
            object initialValue = property.GetValue(context.InitialValue);

            MethodInfo propertySetMethod = property.GetSetMethod(true);

            VisualControlInfo control = CreateControlForType(property.PropertyType, new VisualControlContext(context.SpinBoxes, initialValue, v =>
            {
                property.SetValue(context.InitialValue, v);
                context.ValueChanged(context.InitialValue);
            }));

            if (control.VisualControl != null)
            {
                propertyControls.Add(control.VisualControl);

                control.VisualControl.SetEditable(propertySetMethod != null);

                vbox.AddChild(CreateHBoxForMember(property.Name, control.VisualControl.Control));
            }
        }
    }

    private static void AddFields(VBoxContainer vbox, Type type, VisualControlContext context, out List<IVisualControl> fieldControls, out FieldInfo[] fields)
    {
        fieldControls = [];

        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;

        fields = type
            .GetFields(flags)
            // Exclude delegate types
            .Where(f => !(typeof(Delegate).IsAssignableFrom(f.FieldType)))
            // Exclude fields created by properties
            .Where(f => !f.Name.StartsWith('<') || !f.Name.EndsWith(">k__BackingField"))
            .ToArray();

        foreach (FieldInfo field in fields)
        {
            object initialValue = field.GetValue(context.InitialValue);

            VisualControlInfo control = CreateControlForType(field.FieldType, new VisualControlContext(context.SpinBoxes, initialValue, v =>
            {
                field.SetValue(context.InitialValue, v);
                context.ValueChanged(context.InitialValue);
            }));

            if (control.VisualControl != null)
            {
                fieldControls.Add(control.VisualControl);

                control.VisualControl.SetEditable(!field.IsLiteral);

                vbox.AddChild(CreateHBoxForMember(field.Name, control.VisualControl.Control));
            }
        }
    }

    private static void AddMethods(VBoxContainer vbox, Type type, VisualControlContext context)
    {
        BindingFlags flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly;

        // Cannot include private methods or else we will see Godots built in methods
        MethodInfo[] methods = type.GetMethods(flags)
            // Exclude delegates
            .Where(m => !(typeof(Delegate).IsAssignableFrom(m.ReturnType)))
            // Exclude auto property methods
            .Where(m => !m.Name.StartsWith("get_") && !m.Name.StartsWith("set_"))
            // Exclude the override string ToString() method
            .Where(m => m.Name != "ToString")
            .ToArray();

        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] paramInfos = method.GetParameters();
            object[] providedValues = new object[paramInfos.Length];

            HBoxContainer hboxParams = VisualMethods.CreateMethodParameterControls(method, context.SpinBoxes, providedValues);
            Button button = VisualMethods.CreateMethodButton(method, context.InitialValue, paramInfos, providedValues);

            vbox.AddChild(hboxParams);
            vbox.AddChild(button);
        }
    }

    private static HBoxContainer CreateHBoxForMember(string memberName, Control control)
    {
        HBoxContainer hbox = new();
        hbox.AddChild(new Label { Text = memberName.ToPascalCase().AddSpaceBeforeEachCapital() });
        hbox.AddChild(control);
        return hbox;
    }
}

public class ClassControl(VBoxContainer vboxContainer, List<IVisualControl> visualPropertyControls, List<IVisualControl> visualFieldControls, PropertyInfo[] properties, FieldInfo[] fields) : IVisualControl
{
    public void SetValue(object value)
    {
        Type type = value.GetType();

        for (int i = 0; i < properties.Length; i++)
        {
            object propValue = properties[i].GetValue(value);
            visualPropertyControls[i].SetValue(propValue);
        }

        for (int i = 0; i < fields.Length; i++)
        {
            object fieldValue = fields[i].GetValue(value);
            visualFieldControls[i].SetValue(fieldValue);
        }
    }

    public Control Control => vboxContainer;

    public void SetEditable(bool editable)
    {
        foreach (IVisualControl visualControl in visualPropertyControls)
        {
            visualControl.SetEditable(editable);
        }

        foreach (IVisualControl visualControl in visualFieldControls)
        {
            visualControl.SetEditable(editable);
        }
    }
}
