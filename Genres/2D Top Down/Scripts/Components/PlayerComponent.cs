﻿using Godot;

namespace Template.TopDown2D;

[GlobalClass, Icon(Images.GearIcon)]
public partial class PlayerComponent : EntityComponent
{
    public override void TakeDamage(Vector2 direction = default)
    {
        base.TakeDamage();

        if (_entity is Player player)
        {
            player.ApplyExternalForce(direction * 200);
        }
    }
}
