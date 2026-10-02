using System.Text;
using AmongUs.GameOptions;
using Il2CppInterop.Runtime.Attributes;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.GameOver;
using LaunchpadRevamped.Options.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.GameOptions;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Neutral;

public class ReaperRole(System.IntPtr ptr) : RoleBehaviour(ptr), INeutralRole
{
    public Color RoleColor => LaunchpadPalette.ReaperColor;
    public override bool IsDead => false;

    public int collectedSouls;

    public CustomRoleConfiguration Configuration => new(this)
    {
        TasksCountForProgress = false,
        CanUseVent = false,
        GhostRole = (RoleTypes)RoleId.Get<NeutralGhostRole>(),
        Icon = LaunchpadAssets.SoulButton,
        OptionsScreenshot = LaunchpadAssets.JesterBanner,
    };

    [HideFromIl2Cpp]
    public StringBuilder SetTabText()
    {
        var sb = CustomRoleUtils.CreateForRole(this);
        sb.Append($"\n<b>{collectedSouls}/{OptionGroupSingleton<ReaperOptions>.Instance.SoulCollections} souls collected.");
        return sb;
    }

    public override void AppendTaskHint(Il2CppSystem.Text.StringBuilder taskStringBuilder)
    {
        // remove default task hint
    }

    public override bool DidWin(GameOverReason reason)
    {
        return reason == CustomGameOver.GameOverReason<ReaperGameOver>();
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
