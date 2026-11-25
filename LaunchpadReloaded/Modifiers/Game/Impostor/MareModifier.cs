using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Mare). While the lights are out you move faster and your kill cooldown is short. The cooldown part is
/// in ExtraModifierPatches (SetKillTimer); the speed is only changed on the owner's client.
/// </summary>
public sealed class MareModifier : ExtraModifier
{
    protected override string Key => "mare";
    protected override string DisplayName => "Mare";

    protected override string Description =>
        $"While the lights are out you move {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.MareSpeedBoost:0.0}x faster " +
        $"and your kill cooldown is only {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.MareKillCooldown:0.#}s.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.MareChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    private bool _boosted;
    private float _factor = 1f;

    public override void FixedUpdate()
    {
        if (!Player || !Player.AmOwner || !Player.Data)
        {
            return;
        }

        var dark = !MeetingHud.Instance && !Player.Data.IsDead && ModifierHelpers.IsLightsOut();
        if (dark && !_boosted)
        {
            Begin();
        }
        else if (!dark && _boosted)
        {
            End();
        }
    }

    private void Begin()
    {
        var options = OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance;
        _boosted = true;
        _factor = 1f + options.MareSpeedBoost;
        Player.MyPhysics.Speed *= _factor;
        Player.SetKillTimer(Mathf.Min(Player.killTimer, options.MareKillCooldown));
    }

    private void End()
    {
        if (!_boosted)
        {
            return;
        }

        _boosted = false;
        Player.MyPhysics.Speed /= _factor;
    }

    public override void OnMeetingStart() => End();

    public override void OnDeactivate() => End();

    public override void OnDeath(DeathReason reason)
    {
        End();
        base.OnDeath(reason);
    }
}
