using System.Linq;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Ported from TOHO (Stubborn). The Eraser cannot erase your role or modifiers. The Eraser's list of players to erase is
/// cleaned before the Eraser's own handler uses it (lower priority runs first).
/// </summary>
public sealed class StubbornModifier : ExtraModifier
{
    protected override string Key => "stubborn";
    protected override string DisplayName => "Stubborn";
    protected override string Description => "The Eraser cannot erase your role or your modifiers.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.StubbornChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);

    [RegisterEvent(-100)]
    public static void OnRoundStart(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro)
        {
            return;
        }

        foreach (var id in EraserRole.Pending.ToArray())
        {
            var player = GameData.Instance.GetPlayerById(id)?.Object;
            if (player && player!.HasModifier<StubbornModifier>())
            {
                EraserRole.Pending.Remove(id);
            }
        }
    }
}
