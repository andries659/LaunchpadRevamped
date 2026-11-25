using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Circumvent). A downside for balance: the Impostor can no longer use vents.
/// </summary>
public sealed class CircumventModifier : ExtraModifier
{
    protected override string Key => "circumvent";
    protected override string DisplayName => "Circumvent";
    protected override string Description => "You can no longer use vents.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.CircumventChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    public override bool? CanVent() => false;
}
