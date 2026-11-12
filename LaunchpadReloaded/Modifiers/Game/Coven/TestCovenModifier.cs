using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Placeholder Coven modifier. It does nothing yet; it only exists so the Coven modifier category has something in it
/// to assign, show in the settings and list on the wiki.
/// </summary>
public sealed class TestCovenModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.testCoven");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.testCoven.TabDescription");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CovenModifierOptions>.Instance.TestCovenChance;
    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);
}
