using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Watcher). In meetings you see who everyone voted for, even when votes are anonymous.
/// The effect is in ExtraModifierPatches.
/// </summary>
public sealed class WatcherModifier : ExtraModifier
{
    protected override string Key => "watcher";
    protected override string DisplayName => "Watcher";
    protected override string Description => "In meetings you can see who everyone voted for, even when votes are anonymous.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.WatcherChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);
}
