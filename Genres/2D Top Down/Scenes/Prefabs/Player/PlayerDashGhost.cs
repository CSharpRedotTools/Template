using Godot;
using GodotUtils;

namespace Template.TopDown2D;

public partial class PlayerDashGhost : Node2D
{
    [OnInstantiate]
    private void Init(Vector2 position, AnimatedSprite2D spriteToClone)
    {
        Position = position;

        AnimatedSprite2D sprite = (AnimatedSprite2D)spriteToClone.Duplicate();
        sprite.Material = null;
        AddChild(sprite);
    }

    public override void _Ready()
    {
        Name = nameof(PlayerDashGhost);

        const double MODULATE_DURATION = 0.5;

        new RTween(this)
            .Animate(Node2D.PropertyName.Modulate, Colors.Transparent, MODULATE_DURATION).EaseOut()
            .Callback(QueueFree);
    }
}

