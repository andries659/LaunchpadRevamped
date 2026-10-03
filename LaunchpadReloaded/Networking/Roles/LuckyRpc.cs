using AmongUs.GameOptions;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Crewmate;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class LuckyRpc
{
    /// <summary>
    /// Sent by the host when a Lucky dodges a kill attempt. Gives the attacker feedback and a fresh cooldown.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.LuckyDodge)]
    public static void RpcLuckyDodge(this PlayerControl lucky, PlayerControl attacker)
    {
        if (lucky.Data.Role is not LuckyRole)
        {
            return;
        }

        if (attacker && attacker.AmOwner)
        {
            attacker.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            SoundManager.Instance.PlaySound(LaunchpadAssets.BuzzerSound.LoadAsset(), false, 1f);
        }

        if (lucky.AmOwner)
        {
            SoundManager.Instance.PlaySound(LaunchpadAssets.MoneySound.LoadAsset(), false, 2f);
        }
    }
}
