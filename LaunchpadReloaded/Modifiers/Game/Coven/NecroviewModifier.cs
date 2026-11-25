using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Ported from TOHO (Necroview). In meetings the names of dead players are coloured by the team they were on: red for
/// Impostor, cyan for Crewmate, grey for Neutral and purple for the Coven. The effect is in ExtraModifierPatches.
/// </summary>
public sealed class NecroviewModifier : ExtraModifier
{
    protected override string Key => "necroview";
    protected override string DisplayName => "Necroview";
    protected override string Description => "In meetings you see the team of every dead player: red Impostor, cyan Crewmate, grey Neutral, purple Coven.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.NecroviewChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);
}
