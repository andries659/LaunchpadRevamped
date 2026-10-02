using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using System;
using LaunchpadReloaded.Features;
using MiraAPI.GameOptions.OptionTypes;

namespace LaunchpadReloaded.Options;

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

}