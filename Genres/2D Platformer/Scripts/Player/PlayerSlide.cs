using GodotUtils;

namespace Template.Platformer2D.Retro;

public partial class Player
{
    private State Slide()
    {
        State state = new(nameof(Slide));

        state.Enter = () =>
        {

        };

        state.Update = delta =>
        {

        };

        return state;
    }
}

