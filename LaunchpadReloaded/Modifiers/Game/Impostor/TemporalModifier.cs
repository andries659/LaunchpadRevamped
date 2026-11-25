using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Temporal). Every kill adds time to the emergency button cooldown. The event runs on every client,
/// so every client's button cooldown grows by the same amount.
/// </summary>
public sealed class TemporalModifier : ExtraModifier
{
    protected override string Key => "temporal";
    protected override string DisplayName => "Temporal";

    protected override string Description =>
        $"Every kill you make adds {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.TemporalIncrease:0.#}s to the emergency button cooldown.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.TemporalChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        if (@event.Source && @event.Source.HasModifier<TemporalModifier>())
        {
            ModifierHelpers.AddEmergencyCooldown(OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.TemporalIncrease);
        }
    }
}
