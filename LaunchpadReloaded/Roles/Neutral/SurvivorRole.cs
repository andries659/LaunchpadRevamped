using AmongUs.GameOptions;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Neutral;

public class SurvivorRole(System.IntPtr ptr) : RoleBehaviour(ptr), INeutralRole
{
    public Color RoleColor => LaunchpadPalette.SurvivorColor;
    public override bool IsDead => false;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        CanUseVent = false,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = LaunchpadAssets.SurvivorIcon,
        OptionsScreenshot = LaunchpadAssets.JesterBanner,
    };

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    // The Survivor wins together with whoever wins, as long as they are still alive when the game ends.
    public override bool DidWin(GameOverReason gameOverReason)
    {
        return Player && !Player.Data.IsDead;
    }

    public override bool CanUse(IUsable usable)
    {
        if (!GameManager.Instance.LogicUsables.CanUse(usable, Player))
        {
            return false;
        }

        var console = usable.TryCast<Console>();
        return !(console != null) || console.AllowImpostor;
    }
}
