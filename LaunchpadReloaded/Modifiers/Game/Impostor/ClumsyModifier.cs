using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Clumsy). A kill has a chance to miss: the target is untouched and the cooldown starts over.
/// The host decides, like every other kill protection.
/// </summary>
public sealed class ClumsyModifier : ExtraModifier
{
    protected override string Key => "clumsy";
    protected override string DisplayName => "Clumsy";

    protected override string Description =>
        $"Each kill has a {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.ClumsyMissChance:0}% chance to miss. A miss resets your kill cooldown.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.ClumsyChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    [RegisterEvent(10)]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost || @event.IsCancelled || @event.IsIndirectAttack)
        {
            return;
        }

        var source = @event.Source;
        var target = @event.Target;
        if (!source || !target || !source.HasModifier<ClumsyModifier>())
        {
            return;
        }

        if (UnityEngine.Random.value * 100f >= OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.ClumsyMissChance)
        {
            return;
        }

        @event.Cancel();
        target.RpcModifierDefense(source, ModifierDefenseRpc.Miss);
    }
}
