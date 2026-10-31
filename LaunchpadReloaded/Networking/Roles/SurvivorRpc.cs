using AmongUs.GameOptions;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Neutral;
using Reactor.Networking.Attributes;

namespace LaunchpadRevamped.Networking.Roles;

public static class SurvivorRpc
{
    /// <summary>
    /// Sent by the host when a vested Survivor blocks an attack. Gives the attacker feedback and a fresh cooldown.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.VestBlock)]
    public static void RpcVestBlock(this PlayerControl survivor, PlayerControl attacker)
    {
        if (survivor.Data.Role is not SurvivorRole)
        {
            return;
        }

        if (attacker && attacker.AmOwner)
        {
            attacker.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            SoundManager.Instance.PlaySound(LaunchpadAssets.BuzzerSound.LoadAsset(), false, 1f);
        }

        if (survivor.AmOwner)
        {
            SoundManager.Instance.PlaySound(LaunchpadAssets.SwooshSound.LoadAsset(), false, 1f);
        }
    }
}
