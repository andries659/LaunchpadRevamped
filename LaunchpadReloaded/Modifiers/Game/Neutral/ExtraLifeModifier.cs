using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Extra Life). The first attack that would kill you fails, then the modifier is gone.
/// </summary>
public sealed class ExtraLifeModifier : ExtraModifier
{
    protected override string Key => "extraLife";
    protected override string DisplayName => "Extra Life";
    protected override string Description => "The first time someone tries to kill you, you survive.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.ExtraLifeChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);

    [RegisterEvent(40)]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IgnoreDefense)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || !target.HasModifier<ExtraLifeModifier>())
        {
            return;
        }

        @event.Cancel();
        target.RpcModifierDefense(source, ModifierDefenseRpc.ExtraLife);
        target.RpcRemoveModifier<ExtraLifeModifier>();
    }
}
