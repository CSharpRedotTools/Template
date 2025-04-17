using Godot;
using GodotUtils;
using System;
using System.Linq;
using Godot.Collections;

namespace Template.UI;

public partial class OptionsInput : Control
{
    [Export] private OptionsManager _optionsManager;
    private static BtnInfo _btnNewInput; // the btn waiting for new input
    private Dictionary<StringName, Array<InputEvent>> _defaultActions;
    private VBoxContainer _content;

    public override void _Ready()
    {
        _content = GetNode<VBoxContainer>("Scroll/VBox");
        CreateHotkeys();
    }

    public override void _Input(InputEvent @event)
    {
        if (_btnNewInput != null)
        {
            if (Input.IsActionJustPressed(InputActions.RemoveHotkey))
            {
                StringName action = _btnNewInput.Action;

                // Update input map
                InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);

                // Update options
                _optionsManager.Hotkeys.Actions[action].Remove(_btnNewInput.InputEvent);

                // Update UI
                _btnNewInput.Btn.QueueFree();
                _btnNewInput = null;
            }

            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                _btnNewInput.Btn.Text = _btnNewInput.OriginalText;
                _btnNewInput.Btn.Disabled = false;

                if (_btnNewInput.Plus)
                {
                    _btnNewInput.Btn.QueueFree();
                }

                _btnNewInput = null;
                @event.Dispose(); // Object count was increasing a lot when this function was executed
                return;
            }

            if (@event is InputEventMouseButton eventMouseBtn)
            {
                if (!eventMouseBtn.Pressed)
                {
                    HandleInput(eventMouseBtn);
                }
            }
            else if (@event is InputEventKey eventKey && !eventKey.Echo)
            {
                // Only check when the last key was released from the keyboard
                if (!eventKey.Pressed)
                {
                    HandleInput(eventKey);
                }
            }   
        }
        else
        {
            if (Input.IsActionJustPressed(InputActions.UICancel))
            {
                if (SceneManager.CurrentScene.Name == "Options")
                {
                    if (_btnNewInput == null)
                    {
                        Game.SwitchScene(Scene.MainMenu);
                    }
                }
            }
        }

        @event.Dispose(); // Object count was increasing a lot when this function was executed
    }

    private void HandleInput(InputEvent @event)
    {
        StringName action = _btnNewInput.Action;

        // Prevent something very evil from happening!
        if (action == "fullscreen" && @event is InputEventMouseButton eventBtn)
        {
            return;
        }

        // Re-create the button

        // Preserve the index the button was originally at
        int index = _btnNewInput.Btn.GetIndex();

        // Destroy the button
        _btnNewInput.Btn.QueueFree();

        // Create the button
        Button btn = CreateButton(action, @event, _btnNewInput.HBox);
        btn.Disabled = false;

        // Move the button to where it was originally at
        _btnNewInput.HBox.MoveChild(btn, index);

        Dictionary<StringName, Array<InputEvent>> actions = _optionsManager.Hotkeys.Actions;

        // Clear the specific action event
        actions[action].Remove(_btnNewInput.InputEvent);

        // Update the specific action event
        actions[action].Add(@event);

        // Update input map
        if (_btnNewInput.InputEvent != null)
        {
            InputMap.ActionEraseEvent(action, _btnNewInput.InputEvent);
        }

        InputMap.ActionAddEvent(action, @event);

        // No longer waiting for new input
        _btnNewInput = null;
    }

    private static GButton CreateButton(string action, InputEvent inputEvent, HBoxContainer hbox)
    {
        string readable = "";

        if (inputEvent is InputEventKey key)
        {
            readable = key.Readable();
        }
        else if (inputEvent is InputEventMouseButton button)
        {
            readable = $"Mouse {button.ButtonIndex}";
        }

        // Create the button
        GButton btn = new(readable);
        btn.Pressed += () =>
        {
            // Do not do anything if listening for new input
            if (_btnNewInput != null)
            {
                return;
            }

            // Listening for new hotkey to replace old with...
            _btnNewInput = new BtnInfo
            {
                Action = action,
                Btn = btn,
                HBox = hbox,
                InputEvent = inputEvent,
                OriginalText = btn.Text
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Disabled = true;
            btn.Text = "...";
        };

        // Add this button to the hbox container
        hbox.AddChild(btn);
        return btn;
    }

    private static void CreateButtonPlus(string action, HBoxContainer hbox)
    {
        // Create the button
        GButton btn = new("+");
        btn.Pressed += () =>
        {
            // Do not do anything if listening for new input
            if (_btnNewInput != null)
            {
                return;
            }

            // Listening for new hotkey to replace old with...
            _btnNewInput = new BtnInfo
            {
                Action = action,
                Btn = btn,
                HBox = hbox,
                OriginalText = btn.Text,
                Plus = true
            };

            // Give feedback to the user saying we are waiting for new input
            btn.Disabled = true;
            btn.Text = "...";

            CreateButtonPlus(action, hbox);
        };

        // Add this button to the hbox container
        hbox.AddChild(btn);
    }

    private void CreateHotkeys()
    {
        // Loop through the actions in alphabetical order
        foreach (StringName action in _optionsManager.Hotkeys.Actions.Keys.OrderBy(x => x.ToString()))
        {
            string actionStr = action.ToString();

            // Exlucde "remove_hotkey" action
            if (actionStr == "remove_hotkey")
            {
                continue;
            }

            // Exclude all built-in actions
            if (actionStr.StartsWith("ui"))
            {
                continue;
            }

            HBoxContainer hbox = new();

            // For example convert ui_left to UI_LEFT
            string name = action.ToString().ToUpper();

            // Add the action label. For example 'UI Left'
            hbox.AddChild(new GLabel(name)
            {
                HorizontalAlignment = HorizontalAlignment.Left,
                CustomMinimumSize = new Vector2(200, 0)
            });

            // Add all the events after the action label
            HBoxContainer hboxEvents = new();

            Array<InputEvent> events = _optionsManager.Hotkeys.Actions[action];

            foreach (InputEvent @event in events)
            {
                // Handle keys
                if (@event is InputEventKey eventKey)
                {
                    CreateButton(action, eventKey, hboxEvents);
                }

                // Handle mouse buttons
                if (@event is InputEventMouseButton eventMouseBtn)
                {
                    CreateButton(action, eventMouseBtn, hboxEvents);
                }
            }

            CreateButtonPlus(action, hboxEvents);

            hbox.AddChild(hboxEvents);
            _content.AddChild(hbox);
        }
    }

    private void _on_reset_to_defaults_pressed()
    {
        for (int i = 0; i < _content.GetChildren().Count; i++)
        {
            if (_content.GetChild(i) != this)
            {
                _content.GetChild(i).QueueFree();
            }
        }

        _btnNewInput = null;
        _optionsManager.ResetHotkeys();
        CreateHotkeys();
        GC.Collect(); // Object count was increasing a lot when this function was executed
    }
}

public class BtnInfo
{
    public InputEvent InputEvent { get; set; }
    public string OriginalText { get; set; }
    public StringName Action { get; set; }
    public HBoxContainer HBox { get; set; }
    public Button Btn { get; set; }
    public bool Plus { get; set; }
}

