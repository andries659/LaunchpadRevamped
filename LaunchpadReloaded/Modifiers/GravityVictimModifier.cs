using MiraAPI.Modifiers;
using MiraAPI.Translation;
using System;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

public class GravityVictimModifier(PlayerControl gravityGuy) : BaseModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.gravityVictim");
    public override bool HideOnUi => false;

    private float ogSpeed;

    public override void OnActivate()
    {
        ogSpeed = Player.MyPhysics.Speed;
    }

    public override void OnDeactivate()
    {
        Player.MyPhysics.Speed = ogSpeed;
    }

    public override void Update()
    {
        if (!Player || !gravityGuy)
        {
            return;
        }

        var distance = Vector2.Distance(Player.GetTruePosition(), gravityGuy.GetTruePosition());
        var clamp = Math.Clamp(distance, 0.3f, ogSpeed - 0.6f);

        Player.MyPhysics.Speed = clamp;
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}