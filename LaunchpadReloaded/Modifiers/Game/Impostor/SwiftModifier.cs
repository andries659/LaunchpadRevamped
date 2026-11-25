using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Swift). Your kills do not make you jump to the body. The effect is in ExtraModifierPatches.
/// </summary>
public sealed class SwiftModifier : ExtraModifier
{
    protected override string Key => "swift";
    protected override string DisplayName => "Swift";
    protected override string Description => "Your kills do not make you lunge at the victim.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.SwiftChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);
}
