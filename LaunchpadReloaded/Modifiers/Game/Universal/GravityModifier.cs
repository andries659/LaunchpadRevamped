using LaunchpadReloaded.Components;
using MiraAPI.Translation;
using LaunchpadReloaded.Options.Modifiers;
using LaunchpadReloaded.Options.Modifiers.Universal;
using MiraAPI.GameOptions;
using Reactor.Utilities.Extensions;
using UnityEngine;

namespace LaunchpadReloaded.Modifiers.Game.Universal;

public sealed class GravityModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.gravity");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.gravity.TabDescription");
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GravityChance;
    public override int GetAmountPerGame() => 1;

    private GameObject? detectionCircle;

    public override void OnActivate()
    {
        detectionCircle = new GameObject("DetectionCircle");
        detectionCircle.transform.SetParent(Player.transform);
        detectionCircle.transform.localPosition = new Vector3(0, 0, 0);

        var collider = detectionCircle.AddComponent<CircleCollider2D>();
        collider.radius = OptionGroupSingleton<GravityFieldOptions>.Instance.FieldRadius;
        collider.isTrigger = true;

        var gravityComp = detectionCircle.AddComponent<GravityComponent>();
        gravityComp.gravityGuy = Player;

        detectionCircle.gameObject.SetActive(true);
    }

    public override void OnDeactivate()
    {
        detectionCircle?.gameObject.DestroyImmediate();
    }
}