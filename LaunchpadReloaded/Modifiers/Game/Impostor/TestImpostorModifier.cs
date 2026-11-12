using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Placeholder Impostor modifier. It does nothing yet; it only exists so the Impostor modifier category has something in it
/// to assign, show in the settings and list on the wiki.
/// </summary>
public sealed class TestImpostorModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.testImpostor");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.testImpostor.TabDescription");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ImpostorModifierOptions>.Instance.TestImpostorChance;
    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);
}
