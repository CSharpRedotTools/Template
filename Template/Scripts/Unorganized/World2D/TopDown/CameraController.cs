using Godot;
using GodotUtils;

namespace Template.Unorganized.Topdown;

/*
 * Attach this script to a child node of the Camera node
 * For World2D scenarios.
 */
public partial class CameraController : Node
{
    // Inspector
    [Export]
    private float _speed = 100;

    [ExportGroup("Zoom")]
    [Export(PropertyHint.Range, "0.02, 0.16")]
    private float _zoomIncrementDefault = 0.02f;

    [Export(PropertyHint.Range, "0.01, 10")]
    private float _minZoom = 0.01f;

    [Export(PropertyHint.Range, "0.1, 10")]
    private float _maxZoom = 1.0f;

    [Export(PropertyHint.Range, "0.01, 1")]
    private float _smoothFactor = 0.25f;
    private float _zoomIncrement = 0.02f;
    private float _targetZoom;

    // Panning
    private Vector2 _initialPanPosition;
    private bool _panning;
    private Camera2D _camera;

    public override void _Ready()
    {
        _camera = GetParent<Camera2D>();

        // Make sure the camera zoom does not go past MinZoom
        // Note that a higher MinZoom value means the camera can zoom out more
        float maxZoom = Mathf.Max(_camera.Zoom.X, _minZoom);
        _camera.Zoom = Vector2.One * maxZoom;

        // Set the initial target zoom value on game start
        _targetZoom = _camera.Zoom.X;
    }

    public override void _Process(double delta)
    {
        // Not sure if the below code should be in _PhysicsProcess or _Process

        // Arrow keys and WASD move camera around
        Vector2 dir = Vector2.Zero;

        if (GInput.IsMovingLeft())
        {
            dir.X -= 1;
        }

        if (GInput.IsMovingRight())
        {
            dir.X += 1;
        }

        if (GInput.IsMovingUp())
        {
            dir.Y -= 1;
        }

        if (GInput.IsMovingDown())
        {
            dir.Y += 1;
        }

        if (_panning)
        {
            _camera.Position = _initialPanPosition - (GetViewport().GetMousePosition() / _camera.Zoom.X);
        }

        // Arrow keys and WASD movement are added onto the panning position changes
        _camera.Position += dir.Normalized() * _speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Prevent zoom from becoming too fast when zooming out
        _zoomIncrement = _zoomIncrementDefault * _camera.Zoom.X;

        // Lerp to the target zoom for a smooth effect
        _camera.Zoom = _camera.Zoom.Lerp(new Vector2(_targetZoom, _targetZoom), _smoothFactor);
    }

    // Not sure if this should be done in _Input or _UnhandledInput
    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseButton mouseButton)
        {
            InputEventMouseButton(mouseButton);
        }

        @event.Dispose(); // Object count was increasing a lot when this function was executed
    }

    private void InputEventMouseButton(InputEventMouseButton @event)
    {
        HandlePan(@event);
        HandleZoom(@event);
    }

    private void HandlePan(InputEventMouseButton @event)
    {
        // Left click to start panning the camera
        if (@event.ButtonIndex != MouseButton.Left)
        {
            return;
        }

        // Is this the start of a left click or is this releasing a left click?
        if (@event.IsPressed())
        {
            // Save the intial position
            _initialPanPosition = _camera.Position + (GetViewport().GetMousePosition() / _camera.Zoom.X);
            _panning = true;
        }
        else
        {
            // Only stop panning once left click has been released
            _panning = false;
        }
    }

    private void HandleZoom(InputEventMouseButton @event)
    {
        // Not sure why or if this is required
        if (!@event.IsPressed())
        {
            return;
        }

        // Zoom in
        if (@event.ButtonIndex == MouseButton.WheelUp)
        {
            _targetZoom += _zoomIncrement;
        }

        // Zoom out
        if (@event.ButtonIndex == MouseButton.WheelDown)
        {
            _targetZoom -= _zoomIncrement;
        }

        // Clamp the zoom
        _targetZoom = Mathf.Clamp(_targetZoom, _minZoom, _maxZoom);
    }
}

