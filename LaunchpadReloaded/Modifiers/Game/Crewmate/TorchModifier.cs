using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.Translation;
using LaunchpadRevamped.Options.Modifiers.Crewmate;
using MiraAPI.GameOptions;

namespace LaunchpadRevamped.Modifiers.Game.Crewmate;

public sealed class TorchModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.torch");
    public override string GetDescription() => MiraLocaleManager.Get(
        OptionGroupSingleton<TorchOptions>.Instance.UseFlashlight
            ? "launchpad.modifier.torch.TabDescription.flashlight"
            : "launchpad.modifier.torch.TabDescription.maxVision");

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.TorchChance;
    public override int GetAmountPerGame() => 1;
}