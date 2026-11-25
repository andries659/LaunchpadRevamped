using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Subversion). The player gets the sabotage button even though their role normally has none.
/// The Coven version is the Saboteur. The button handling is in ExtraModifierPatches.
/// </summary>
public sealed class SubversionModifier : ExtraModifier
{
    protected override string Key => "subversion";
    protected override string DisplayName => "Subversion";
    protected override string Description => "You can use sabotages, even though your role normally cannot.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.SubversionChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);
}
