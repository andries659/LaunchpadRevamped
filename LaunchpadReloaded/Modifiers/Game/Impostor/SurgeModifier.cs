using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Every kill you make gives you a burst of speed for a few seconds, to get away from the body.
/// The speed is only changed on the owner's client; everyone else just sees the player move.
/// </summary>
public sealed class SurgeModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.surge");
    public override string GetDescription() => MiraLocaleManager.GetParsed(
        "launchpad.modifier.surge.TabDescription",
        new[]
        {
            ("{boost}", OptionGroupSingleton<SurgeOptions>.Instance.SpeedBoost.ToString("0.0")),
            ("{seconds}", OptionGroupSingleton<SurgeOptions>.Instance.Duration.ToString("0")),
        });

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.SurgeChance;
    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    private bool _boosted;
    private float _factor = 1f;
    private float _boostEnd;

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        @event.Source.GetModifier<SurgeModifier>()?.StartBoost();
    }

    private void StartBoost()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        var options = OptionGroupSingleton<SurgeOptions>.Instance;
        _boostEnd = Time.time + options.Duration;

        if (_boosted)
        {
            return;
        }

        _boosted = true;
        _factor = options.SpeedBoost;
        Player.MyPhysics.Speed *= _factor;
    }

    private void EndBoost()
    {
        if (!_boosted)
        {
            return;
        }

        _boosted = false;
        Player.MyPhysics.Speed /= _factor;
    }

    public override void FixedUpdate()
    {
        if (_boosted && Time.time >= _boostEnd)
        {
            EndBoost();
        }
    }

    public override void OnMeetingStart()
    {
        EndBoost();
    }

    public override void OnDeactivate()
    {
        EndBoost();
    }
}
