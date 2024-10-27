using Godot;
using RedotUtils;

namespace Template.TopDown2D;

public partial class PlayerCamera : Camera2D
{
    private Player _player;

	public override void _Ready()
	{
        PositionSmoothingEnabled = false;
        SetPhysicsProcess(false);
    }

	public override void _PhysicsProcess(double delta)
	{
        Position = _player.Position;
    }

    public void StartFollowingPlayer(Player player)
    {
        _player = player;
        Position = player.Position;
        RTween.Delay(this, 0.01, () => PositionSmoothingEnabled = true);
        SetPhysicsProcess(true);
    }

    public void StopFollowingPlayer()
    {
        SetPhysicsProcess(false);
    }
}

