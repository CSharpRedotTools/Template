using Godot;

namespace Template;

[GlobalClass]
public partial class FrogResource : Resource
{
    [Export] public float JumpForceInfluence { get; set; } = 2;
    [Export] public Color Color { get; set; } = Colors.White;
}