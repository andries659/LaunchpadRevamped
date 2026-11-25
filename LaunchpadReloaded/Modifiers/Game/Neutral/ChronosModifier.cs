using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Chronos). While a Chronos is alive, the discussion time of every meeting is bent. The effect is in
/// ExtraModifierPatches.
/// </summary>
public sealed class ChronosModifier : ExtraModifier
{
    protected override string Key => "chronos";
    protected override string DisplayName => "Chronos";

    protected override string Description =>
        $"If you call a meeting, the discussion time is cut by {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.ChronosCut:0}%. " +
        $"If someone else calls a meeting while you are alive, it lasts {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.ChronosExtend:0}s longer.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.ChronosChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);
}
