using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Universal;

/// <summary>
/// Ported from TOHO (Fragile). Any attack kills you, because the attack ignores every defense: shields, the Lucky dodge,
/// the Jinx and Extra Life do not work against it. Runs before the other kill handlers so they see the flag.
/// </summary>
public sealed class FragileModifier : ExtraModifier
{
    protected override string Key => "fragile";
    protected override string DisplayName => "Fragile";
    protected override string Description => "If someone attacks you, you die instantly. Shields, dodges and extra lives do not protect you.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraUniversalModifierOptions>.Instance.FragileChance;

    [RegisterEvent(-100)]
    public static void OnBeforeMurder(BeforeMurderEvent @event)
    {
        if (@event.Target && @event.Target.HasModifier<FragileModifier>())
        {
            @event.IgnoreDefense = true;
        }
    }
}
