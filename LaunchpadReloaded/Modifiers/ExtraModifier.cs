using MiraAPI.PluginLoading;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// Base class of the modifiers ported from TOHO. The name and description come from the locale files when a
/// translation exists and fall back to the English text written in the class.
/// </summary>
[MiraIgnore]
public abstract class ExtraModifier : LPModifier
{
    protected abstract string Key { get; }
    protected abstract string DisplayName { get; }
    protected abstract string Description { get; }

    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier." + Key, DisplayName);

    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier." + Key + ".TabDescription", Description);
}
