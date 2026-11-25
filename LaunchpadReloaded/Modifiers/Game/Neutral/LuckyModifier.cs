using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Lucky). Every attack has a chance to be dodged. The host decides; the attacker gets a fresh cooldown.
/// </summary>
public sealed class LuckyModifier : ExtraModifier
{
    protected override string Key => "luckyCharm";
    protected override string DisplayName => "Lucky";

    protected override string Description =>
        $"Every attack on you has a {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.LuckyDodgeChance:0}% chance to fail.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.LuckyChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);

    [RegisterEvent(30)]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IgnoreDefense)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || !target.HasModifier<LuckyModifier>())
        {
            return;
        }

        if (UnityEngine.Random.value * 100f >= OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.LuckyDodgeChance)
        {
            return;
        }

        @event.Cancel();
        target.RpcModifierDefense(source, ModifierDefenseRpc.Dodge);
    }
}
