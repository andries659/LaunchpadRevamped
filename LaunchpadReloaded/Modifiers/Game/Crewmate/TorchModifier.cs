using LaunchpadReloaded.Options.Modifiers;
using MiraAPI.Translation;
using LaunchpadReloaded.Options.Modifiers.Crewmate;
using MiraAPI.GameOptions;

namespace LaunchpadReloaded.Modifiers.Game.Crewmate;

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