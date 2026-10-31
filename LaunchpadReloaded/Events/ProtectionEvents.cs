using LaunchpadRevamped.Modifiers;
using LaunchpadRevamped.Networking.Roles;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using Random = UnityEngine.Random;

namespace LaunchpadRevamped.Events;

/// <summary>
/// Kill protection: the Survivor's vest and the Lucky's dodge.
///
/// MiraAPI performs kills inside its own CheckMurder replacement, so a plain Harmony prefix on CheckMurder can't stop
/// them. BeforeMurderEvent is raised before the kill and can cancel it, for vanilla and custom murders alike.
/// The host decides, since it is the one that confirms kills.
/// </summary>
public static class ProtectionEvents
{
    [RegisterEvent]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IgnoreDefense)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || !target.Data || target.Data.IsDead)
        {
            return;
        }

        // The vest blocks every attack while it is active.
        if (target.HasModifier<VestedModifier>())
        {
            @event.Cancel();
            target.RpcVestBlock(source);
            return;
        }

        // The Lucky only gets a chance against Impostors.
        if (target.Data.Role is LuckyRole lucky && source.Data.Role.IsImpostor && TryDodge(lucky))
        {
            @event.Cancel();
            target.RpcLuckyDodge(source);
        }
    }

    private static bool TryDodge(LuckyRole lucky)
    {
        var options = OptionGroupSingleton<LuckyOptions>.Instance;
        var maxDodges = (int)options.MaxDodges;
        if (maxDodges > 0 && lucky.DodgesUsed >= maxDodges)
        {
            return false;
        }

        if (Random.value * 100f >= options.DodgeChance)
        {
            return false;
        }

        lucky.DodgesUsed++;
        return true;
    }
}
