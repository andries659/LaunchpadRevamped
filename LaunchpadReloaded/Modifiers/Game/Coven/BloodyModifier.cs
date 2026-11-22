using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// If you are killed, your killer leaves a trail of bloody footprints, visible to everyone, for a while.
/// </summary>
public sealed class BloodyModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.bloody");
    public override string GetDescription() => MiraLocaleManager.GetParsed(
        "launchpad.modifier.bloody.TabDescription",
        new() { ["{seconds}"] = OptionGroupSingleton<BloodyOptions>.Instance.TrailDuration.ToString("0") });

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CovenModifierOptions>.Instance.BloodyChance;
    public override int GetAmountPerGame() => 1;

    // The modifier has to survive the death it reacts to.
    public override bool RemoveOnDeath => false;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);

    [RegisterEvent]
    public static void AfterMurderEvent(AfterMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.Source == @event.Target)
        {
            return;
        }

        if (!@event.Target.HasModifier<BloodyModifier>() || @event.Source.HasModifier<BloodyTrailModifier>())
        {
            return;
        }

        @event.Source.RpcAddModifier<BloodyTrailModifier>();
    }
}
