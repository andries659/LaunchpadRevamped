using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// The Coven version of Subversion (TOHO): the player gets the sabotage button even though their role normally has none.
/// The button handling is in ExtraModifierPatches.
/// </summary>
public sealed class SaboteurModifier : ExtraModifier
{
    protected override string Key => "saboteur";
    protected override string DisplayName => "Saboteur";
    protected override string Description => "You can use sabotages, even though your role normally cannot.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.SaboteurChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);
}
