using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Placeholder Neutral modifier. It does nothing yet; it only exists so the Neutral modifier category has something in it
/// to assign, show in the settings and list on the wiki.
/// </summary>
public sealed class TestNeutralModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.testNeutral");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.testNeutral.TabDescription");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<NeutralModifierOptions>.Instance.TestNeutralChance;
    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);
}
