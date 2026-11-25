using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Ported from TOHO (Loyal). You cannot be recruited or turned: the Vampire's bite and the Puppeteer's control fail on you.
/// The checks are in ExtraModifierPatches.
/// </summary>
public sealed class LoyalModifier : ExtraModifier
{
    protected override string Key => "loyal";
    protected override string DisplayName => "Loyal";
    protected override string Description => "You cannot be turned. The Vampire's bite and the Puppeteer's control do nothing to you.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.LoyalChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);
}
