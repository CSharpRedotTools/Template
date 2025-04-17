#if TOOLS
using Godot;
using GodotUtils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Template;

public partial class CheckExports : Node
{
    public override void _Ready()
    {
        List<Node> nodes = GetTree().Root.GetChildren<Node>();

        foreach (Node node in nodes)
        {
            Type type = node.GetType();

            CheckFields(type, node);
            CheckProperties(type, node);
        }
    }

    private void CheckFields(Type type, Node node)
    {
        foreach (FieldInfo field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (field.GetCustomAttribute<ExportAttribute>() != null)
            {
                object value = field.GetValue(node);

                if (value == null)
                {
                    string visibility = field.IsPublic ? "public" : "private";
                    LogWarning(visibility, field.FieldType.Name, field.Name, type.Name);
                }
            }
        }
    }

    private void CheckProperties(Type type, Node node)
    {
        foreach (PropertyInfo property in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            if (property.GetCustomAttribute<ExportAttribute>() != null && property.CanRead && property.CanWrite)
            {
                object value = property.GetValue(node);

                if (value == null)
                {
                    string visibility = property.GetMethod.IsPublic ? "public" : "private";
                    LogWarning(visibility, property.PropertyType.Name, property.Name, type.Name);
                }
            }
        }
    }

    private void LogWarning(string visibilityModifier, string memberTypeName, string memberName, string typeName)
    {
        _pushWarningMessage = $"[Export] {visibilityModifier} {memberTypeName} {memberName}; in {typeName}.cs is null and needs to be set!";
        _();

        GD.PrintRich(
            $"[color=orange]" +
            $"[color=yellow][Export] {visibilityModifier} {memberTypeName} {memberName};[/color] " +
            $"in [color=yellow]{typeName}.cs[/color] " +
            $"is null and needs to be set!" +
            $"[/color]"
        );
    }

    private string _pushWarningMessage;

    /// <summary>
    /// Godot's PushWarning displays the functions params and function name in the message taking up a lot 
    /// of space so that is why this extra function was created. 
    /// </summary>
    private void _()
    {
        // Example of message shown:
        // void Template.CheckExports._(): [Export] private Node _test; in Player.cs is null and needs to be set!
        GD.PushWarning(_pushWarningMessage);
    }
}
#endif
