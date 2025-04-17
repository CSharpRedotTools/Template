using Godot;
using GodotUtils;

namespace Template.Platformer2D.Retro;

public partial class Player
{
    private State Idle()
    {
        State state = new(nameof(Idle));

        state.Enter = () =>
        {
            Sprite.Play("idle");
        };

        state.Update = delta =>
        {
            if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
            {
                SwitchState(Jump());
            }

            else if (Input.IsActionJustPressed(InputActions.MoveDown) && GetFloorAngle() > 0)
            {

            }
        };

        return state;
    }
}

