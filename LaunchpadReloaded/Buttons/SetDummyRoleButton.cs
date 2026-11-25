using AmongUs.GameOptions;
using LaunchpadRevamped.Components;
using LaunchpadRevamped.Features;
using MiraAPI.Keybinds;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Buttons;

/// <summary>
/// Freeplay only: opens the role picker and gives the chosen role to the nearest other player (a dummy), so roles
/// like the Eraser can be tested against any role without relaunching.
/// </summary>
public class SetDummyRoleButton : BaseLaunchpadButton<PlayerControl>
{
    public override string Name => "launchpad.button.setDummyRole";
    public override float Cooldown => 0;
    public override float EffectDuration => 0;
    public override int MaxUses => 0;
    public override LoadableAsset<Sprite> Sprite => LaunchpadAssets.SetDummyRoleButton;
    public override bool TimerAffectedByPlayer => false;
    public override bool AffectedByHack => false;

    public override BaseKeybind Keybind => MiraGlobalKeybinds.SecondaryAbility;

    public override bool Enabled(RoleBehaviour? role)
    {
        return TutorialManager.InstanceExists;
    }

    public override PlayerControl? GetTarget()
    {
        PlayerControl? best = null;
        var bestDistance = Distance;
        var local = PlayerControl.LocalPlayer;

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.AmOwner || !player.Data || player.Data.IsDead)
            {
                continue;
            }

            var distance = Vector2.Distance(local.GetTruePosition(), player.GetTruePosition());
            if (distance <= bestDistance)
            {
                best = player;
                bestDistance = distance;
            }
        }

        return best;
    }

    public override bool IsTargetValid(PlayerControl? target)
    {
        return target && !target!.Data.IsDead;
    }

    public override void SetOutline(bool active)
    {
        Target?.cosmetics.SetOutline(active, new Il2CppSystem.Nullable<Color>(Color.white));
    }

    protected override void OnClick()
    {
        if (Target == null || Minigame.Instance)
        {
            return;
        }

        var dummy = Target;
        ResetTarget();

        var minigame = GuessRoleMinigame.Create();
        minigame.Open(
            role => role && !role.IsDead,
            selectedRole =>
            {
                // The picker reports null when it was dismissed.
                if (selectedRole == null)
                {
                    return;
                }

                if (dummy && !dummy.Data.IsDead)
                {
                    dummy.RpcSetRole(selectedRole.Role, true);
                }

                minigame.Close();
            });
    }
}
