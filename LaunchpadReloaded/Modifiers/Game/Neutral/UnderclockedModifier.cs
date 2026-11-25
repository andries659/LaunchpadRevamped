using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Underclocked). A longer kill cooldown, for Neutral roles that use the normal kill button.
/// The cooldown itself is changed in ExtraModifierPatches (SetKillTimer).
/// </summary>
public sealed class UnderclockedModifier : ExtraModifier
{
    protected override string Key => "underclocked";
    protected override string DisplayName => "Underclocked";

    protected override string Description =>
        $"Your kill cooldown is {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.UnderclockedIncrease:0}% longer.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.UnderclockedChance;

    public override bool IsModifierValidOn(RoleBehaviour role) =>
        base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role) && ModifierHelpers.HasVanillaKillButton(role)
        && (!role.Player || !role.Player.HasModifier<OverclockedModifier>());
}
