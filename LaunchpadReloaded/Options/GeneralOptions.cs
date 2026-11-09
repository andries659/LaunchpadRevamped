using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using System;
using LaunchpadRevamped.Features;
using MiraAPI.GameOptions.OptionTypes;

namespace LaunchpadRevamped.Options;

public class GeneralOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.general";
    public override Func<bool> GroupVisible => CustomGameModeManager.IsClassic;

    public ModdedToggleOption Notepad { get; set; } = new("launchpad.options.general.notepad", true)
    {
        ChangedEvent = value =>
        {
            NotepadHud.Instance?.SetNotepadButtonVisible(value);
        }
    };

    [ModdedToggleOption("launchpad.options.general.banCheaters")] public bool BanCheaters { get; set; } = true;
    [ModdedToggleOption("launchpad.options.general.disableMeetingTeleport")] public bool DisableMeetingTeleport { get; set; } = false;
    [ModdedToggleOption("launchpad.options.general.welcomeMessage")] public bool WelcomeMessage { get; set; } = true;
    [ModdedToggleOption("launchpad.options.general.playerTags")] public bool PlayerTags { get; set; } = true;

}