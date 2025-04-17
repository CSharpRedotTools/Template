using Godot;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using System.Threading;
using static Godot.Control;
using GodotUtils;

namespace Template;

/// <summary>
/// The main core class for the visualizer UI
/// </summary>
public static class VisualUI
{
    public const float VISUAL_UI_SCALE_FACTOR = 0.6f;

    /// <summary>
    /// Creates the visual panel for a specified visual node.
    /// </summary>
    public static (Control, List<Action>) CreateVisualPanel(SceneTree tree, VisualNode debugVisualNode)
    {
        Dictionary<Node, VBoxContainer> visualNodes = [];
        
        List<VisualSpinBox> spinBoxes = [];
        List<Action> updateControls = [];

        Node node = debugVisualNode.Node;

        PanelContainer panelContainer = CreatePanelContainer(node.Name);

        VBoxContainer mutableMembers = CreateColoredVBox(0.8f, 1, 0.8f);
        VBoxContainer readonlyMembers = CreateColoredVBox(1.0f, 0.75f, 0.8f);

        string[] visualizeMembers = debugVisualNode.VisualizeMembers;

        AddVisualControls(visualizeMembers, node, readonlyMembers, updateControls, spinBoxes);

        AddMemberInfoElements(mutableMembers, debugVisualNode.Properties, node, spinBoxes);

        AddMemberInfoElements(mutableMembers, debugVisualNode.Fields, node, spinBoxes);

        VisualMethods.AddMethodInfoElements(mutableMembers, debugVisualNode.Methods, node, spinBoxes);

        VBoxContainer vboxLogs = new();
        mutableMembers.AddChild(vboxLogs);

        visualNodes.Add(node, vboxLogs);

        VBoxContainer vboxParent = CreateVBoxParent(node.Name);

        vboxParent.AddChild(readonlyMembers);
        vboxParent.AddChild(mutableMembers);

        SetButtonsToReleaseFocusOnPress(vboxParent);

        panelContainer.AddChild(vboxParent);
        
        // Add to canvas layer so UI is not affected by lighting in game world
        CanvasLayer canvasLayer = CreateCanvasLayer(node.Name, node.GetInstanceId());
        canvasLayer.AddChild(panelContainer);

        tree.Root.CallDeferred(Node.MethodName.AddChild, canvasLayer);

        SetInitialPosition(mutableMembers, debugVisualNode.InitialPosition);

        // This is ugly but I don't know how else to do it
        VisualLogger.VisualNodes = visualNodes;

        return (panelContainer, updateControls);
    }
    
    /// <summary>
    /// Sets the initial position for a VBoxContainer.
    /// </summary>
    private static void SetInitialPosition(VBoxContainer vbox, Vector2 initialPosition)
    {
        if (initialPosition != Vector2.Zero)
        {
            vbox.GlobalPosition = initialPosition;
        }
    }

    /// <summary>
    /// Ensures buttons release focus on press within a VBoxContainer.
    /// </summary>
    private static void SetButtonsToReleaseFocusOnPress(VBoxContainer vboxParent)
    {
        foreach (BaseButton baseButton in vboxParent.GetChildren<BaseButton>())
        {
            baseButton.Pressed += () =>
            {
                _ = new RTween(baseButton)
                    .Delay(0.1)
                    .Callback(() => baseButton.ReleaseFocus());
            };
        }
    }

    /// <summary>
    /// Creates a VBoxContainer parent with a specified name.
    /// </summary>
    private static VBoxContainer CreateVBoxParent(string name)
    {
        VBoxContainer vboxParent = new();

        vboxParent.AddChild(new Label
        {
            Text = name,
            LabelSettings = new LabelSettings
            {
                FontSize = 20,
                FontColor = Colors.LightSkyBlue,
                OutlineColor = Colors.Black,
                OutlineSize = 6,
            },
            HorizontalAlignment = HorizontalAlignment.Center
        });

        return vboxParent;
    }

    /// <summary>
    /// Creates a colored VBoxContainer with specified RGB values.
    /// </summary>
    private static VBoxContainer CreateColoredVBox(float r, float g, float b)
    {
        return new VBoxContainer
        {
            Modulate = new Color(r, g, b, 1)
        };
    }
    
    /// <summary>
    /// Creates a panel container with a specified name.
    /// </summary>
    private static PanelContainer CreatePanelContainer(string name)
    {
        PanelContainer panelContainer = new()
        {
            // Ensure this info is rendered above all game elements
            Name = name,
            Scale = Vector2.One * VISUAL_UI_SCALE_FACTOR,
            ZIndex = (int)RenderingServer.CanvasItemZMax
        };

        panelContainer.AddThemeStyleboxOverride("panel", new StyleBoxEmpty());

        return panelContainer;
    }

    /// <summary>
    /// Attempts to get member information for a Node.
    /// </summary>
    private static void TryGetMemberInfo(Node node, string visualMember, out PropertyInfo property, out FieldInfo field, out object initialValue)
    {
        property = node.GetType().GetProperty(visualMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);
        field = null;
        initialValue = null;

        if (property != null)
        {
            initialValue = property.GetValue(property.GetGetMethod(true).IsStatic ? null : node);
        }
        else
        {
            field = node.GetType().GetField(visualMember, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance);

            if (field != null)
            {
                initialValue = field.GetValue(field.IsStatic ? null : node);
            }
        }
    }

    /// <summary>
    /// Adds visual controls for specified members of a Node.
    /// </summary>
    private static void AddVisualControls(string[] visualizeMembers, Node node, VBoxContainer readonlyMembers, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        if (visualizeMembers == null)
        {
            return;
        }
        
        foreach (string visualMember in visualizeMembers)
        {
            TryGetMemberInfo(node, visualMember, out PropertyInfo property, out FieldInfo field, out object initialValue);

            // If neither property nor field is found, skip this member
            if (property == null && field == null)
            {
                continue;
            }

            if (initialValue != null)
            {
                AddVisualControl(visualMember, readonlyMembers, node, field, property, initialValue, updateControls, spinBoxes);
            }
            else
            {
                _ = TryAddVisualControlAsync(visualMember, readonlyMembers, node, field, property, updateControls, spinBoxes);
            }
        }
    }

    /// <summary>
    /// Creates a canvas layer for a Node with a specified name and instance ID.
    /// </summary>
    private static CanvasLayer CreateCanvasLayer(string name, ulong instanceId)
    {
        CanvasLayer canvasLayer = new();
        canvasLayer.FollowViewportEnabled = true;
        canvasLayer.Name = $"Visualizing {name} {instanceId}";
        return canvasLayer;
    }

    /// <summary>
    /// Asynchronously tries to add a visual control for a Node member.
    /// </summary>
    private static async Task TryAddVisualControlAsync(string visualMember, VBoxContainer readonlyMembers, Node node, FieldInfo field, PropertyInfo property, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        CancellationTokenSource cts = new();
        CancellationToken token = cts.Token;

        int elapsedSeconds = 0;

        while (!token.IsCancellationRequested)
        {
            object value = null;

            if (field != null)
            {
                value = field.GetValue(node);
            }
            else if (property != null)
            {
                value = property.GetValue(node);
            }

            if (value != null)
            {
                AddVisualControl(visualMember, readonlyMembers, node, field, property, value, updateControls, spinBoxes);
                break;
            }

            try
            {
                await Task.Delay(1000, token);
                elapsedSeconds++;

                if (elapsedSeconds == 3)
                {
                    string memberName = string.Empty;

                    if (field != null)
                    {
                        memberName = field.Name;
                    }
                    else if (property != null)
                    {
                        memberName = property.Name;
                    }

                    GD.PrintRich($"[color=orange][Visualize] Tracking '{node.Name}' to see if '{memberName}' value changes[/color]");
                }
            }
            catch (TaskCanceledException)
            {
                // Task was cancelled, exit the loop
                break;
            }
        }
    }

    /// <summary>
    /// Adds a visual control to the UI for a Node member.
    /// </summary>
    private static void AddVisualControl(string visualMember, VBoxContainer readonlyMembers, Node node, FieldInfo field, PropertyInfo property, object initialValue, List<Action> updateControls, List<VisualSpinBox> spinBoxes)
    {
        Type memberType = property != null ? property.PropertyType : field.FieldType;

        VisualControlContext context = new(spinBoxes, initialValue, v =>
        {
            // Do nothing
        });

        VisualControlInfo visualControlInfo = VisualControlTypes.CreateControlForType(memberType, context);

        visualControlInfo.VisualControl.SetEditable(false);

        updateControls.Add(() =>
        {
            object newValue = property != null
                ? property.GetValue(property.GetGetMethod(true).IsStatic ? null : node)
                : field.GetValue(field.IsStatic ? null : node);

            visualControlInfo.VisualControl.SetValue(newValue);
        });

        HBoxContainer hbox = new();
        hbox.AddChild(new Label { Text = visualMember });
        hbox.AddChild(visualControlInfo.VisualControl.Control);
        readonlyMembers.AddChild(hbox);
    }

    /// <summary>
    /// Adds member information elements to a VBoxContainer.
    /// </summary>
    private static void AddMemberInfoElements(VBoxContainer vbox, IEnumerable<MemberInfo> members, Node node, List<VisualSpinBox> spinBoxes)
    {
        foreach (MemberInfo member in members)
        {
            Control element = CreateMemberInfoElement(member, node, spinBoxes);
            vbox.AddChild(element);
        }
    }

    /// <summary>
    /// Creates a member information element for a specified Node member.
    /// </summary>
    private static HBoxContainer CreateMemberInfoElement(MemberInfo member, Node node, List<VisualSpinBox> spinBoxes)
    {
        HBoxContainer hbox = new();

        Type type = VisualHandler.GetMemberType(member);

        object initialValue = VisualHandler.GetMemberValue(member, node);

        if (initialValue == null)
        {
            PrintUtils.Warning($"[Visualize] '{member.Name}' value in '{node.Name}' is null");
            return hbox;
        }

        VisualControlInfo element = VisualControlTypes.CreateControlForType(type, new VisualControlContext(spinBoxes, initialValue, v => 
        {
            VisualHandler.SetMemberValue(member, node, v);
        }));

        if (element.VisualControl != null)
        {
            Label label = new()
            {
                Text = member.Name.ToPascalCase().AddSpaceBeforeEachCapital(),
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };

            hbox.AddChild(label);
            hbox.AddChild(element.VisualControl.Control);
        }

        return hbox;
    }
}

