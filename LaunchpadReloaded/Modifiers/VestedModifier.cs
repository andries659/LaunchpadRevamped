using LaunchpadRevamped.Options.Roles.Neutral;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// Added by the Survivor's vest. While it is active, attacks on the player are blocked (see ProtectionEvents).
/// The owner's client removes it for everyone when the duration is over, so all clients agree on when it ends.
/// </summary>
public sealed class VestedModifier : BaseModifier
{
    private float _elapsed;
    private bool _removing;

    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.vested");

    public override void FixedUpdate()
    {
        if (!Player.AmOwner || _removing)
        {
            return;
        }

        _elapsed += Time.fixedDeltaTime;
        var expired = _elapsed >= OptionGroupSingleton<SurvivorOptions>.Instance.VestDuration;

        if (expired || MeetingHud.Instance || Player.Data.IsDead)
        {
            _removing = true;
            Player.RpcRemoveModifier<VestedModifier>();
        }
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }
}
