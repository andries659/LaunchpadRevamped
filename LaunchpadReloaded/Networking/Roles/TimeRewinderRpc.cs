using System.Collections;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Roles.Crewmate;
using LaunchpadRevamped.Roles.Crewmate;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using Reactor.Networking.Attributes;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Networking.Roles;

public static class TimeRewinderRpc
{
    /// <summary>
    /// Every client runs this and moves only its own player back to where that player was a few seconds ago,
    /// using its own recorded history.
    /// </summary>
    [MethodRpc((uint)LaunchpadRpc.Rewind)]
    public static void RpcRewind(this PlayerControl rewinder)
    {
        if (rewinder.Data.Role is not TimeRewinderRole)
        {
            rewinder.KickForCheating();
            return;
        }

        var local = PlayerControl.LocalPlayer;
        if (!local || !local.Data || local.Data.IsDead || local.Data.Disconnected || local.inVent)
        {
            return;
        }

        var options = OptionGroupSingleton<TimeRewinderOptions>.Instance;
        if (local != rewinder && !options.RewindEveryone)
        {
            return;
        }

        if (!RewindHistory.TryGetPosition(options.RewindDuration, out var position))
        {
            return;
        }

        // Otherwise a task could be finished from wherever the player got sent back to.
        if (Minigame.Instance)
        {
            Minigame.Instance.Close();
        }

        local.NetTransform.RpcSnapTo(position);

        SoundManager.Instance.PlaySound(LaunchpadAssets.SwooshSound.LoadAsset(), false, 1f);
        Coroutines.Start(CoFlash());
    }

    private static IEnumerator CoFlash()
    {
        const float duration = 0.6f;

        var fullScreen = HudManager.Instance.FullScreen;
        fullScreen.gameObject.SetActive(true);

        var elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            fullScreen.color = new UnityEngine.Color(0.35f, 0.65f, 1f, Mathf.Lerp(0.5f, 0f, elapsed / duration));
            yield return null;
        }

        fullScreen.gameObject.SetActive(false);
    }
}
