using Godot;
using RedotUtils;
using Template.UI;

namespace Template.FPS3D;

public partial class Player : CharacterBody3D
{
    [Export] private OptionsManager _options;
    private Vector3 _cameraTarget;
    private Vector2 _mouseInput;
    private float _mouseSensitivity;

    private void OnReadyUI()
    {
        _mouseSensitivity = _options.Options.MouseSensitivity * 0.0001f;

        OptionsGameplay gameplay = GetNode<UIPopupMenu>("%PopupMenu")
            .Options.GetNode<OptionsGameplay>("%Gameplay");

        gameplay.OnMouseSensitivityChanged += value =>
        {
            _mouseSensitivity = value * 0.0001f;
        };
    }

    private void OnPhysicsProcessUI()
    {
        if (Input.IsActionJustPressed(InputActions.NextHeldItem))
        {
            _animTree.SetCondition("holster", true);
        }

        if (Input.IsActionJustPressed(InputActions.PreviousHeldItem))
        {
            //animTree.SetCondition("holster", true);
        }
    }

    private void OnInputUI(InputEvent @event)
    {
        if (Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            return;
        }

        if (@event is InputEventMouseMotion motion)
        {
            _mouseInput = motion.Relative;

            _cameraTarget += new Vector3(
                -motion.Relative.Y * _mouseSensitivity,
                -motion.Relative.X * _mouseSensitivity, 0);

            // Prevent camera from looking too far up or down
            Vector3 rotDeg = _cameraTarget;
            rotDeg.X = Mathf.Clamp(rotDeg.X, -89f.ToRadians(), 89f.ToRadians());
            _cameraTarget = rotDeg;
        }
    }
}

