using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Options.Roles.Impostor;
using LaunchpadRevamped.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Reactor.Networking.Attributes;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class GrenadierRpc
{
    [MethodRpc((uint)LaunchpadRpc.Blind)]
    public static void RpcBlind(this PlayerControl grenadier)
    {
        if (grenadier.Data.Role is not GrenadierRole)
        {
            grenadier.KickForCheating();
            return;
        }

        // Every client runs this and only decides for itself, so the effect is purely local
        // to the blinded player and doesn't depend on other clients' view of positions.
        var local = PlayerControl.LocalPlayer;
        if (!local || local == grenadier || local.Data.IsDead || local.Data.Disconnected || local.HasModifier<BlindedModifier>())
        {
            return;
        }

        var options = OptionGroupSingleton<GrenadierOptions>.Instance;
        if (local.Data.Role.IsImpostor && !options.BlindImpostors)
        {
            return;
        }

        if (Vector2.Distance(local.GetTruePosition(), grenadier.GetTruePosition()) > options.BlindRadius)
        {
            return;
        }

        local.AddModifier<BlindedModifier>();
    }
}
