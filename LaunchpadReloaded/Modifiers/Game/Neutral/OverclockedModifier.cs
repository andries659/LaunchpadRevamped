using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Overclocked). A shorter kill cooldown, for Neutral roles that use the normal kill button.
/// The cooldown itself is changed in ExtraModifierPatches (SetKillTimer).
/// </summary>
public sealed class OverclockedModifier : ExtraModifier
{
    protected override string Key => "overclocked";
    protected override string DisplayName => "Overclocked";

    protected override string Description =>
        $"Your kill cooldown is {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.OverclockedReduction:0}% shorter.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.OverclockedChance;

    public override bool IsModifierValidOn(RoleBehaviour role) =>
        base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role) && ModifierHelpers.HasVanillaKillButton(role)
        && (!role.Player || !role.Player.HasModifier<UnderclockedModifier>());
}
