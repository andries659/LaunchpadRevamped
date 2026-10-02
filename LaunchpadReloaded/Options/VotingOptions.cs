using LaunchpadRevamped.Features.Voting;
using MiraAPI.GameModes;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.GameOptions.OptionTypes;
using MiraAPI.Utilities;
using System;

namespace LaunchpadRevamped.Options;

public class VotingOptions : AbstractOptionGroup
{
    public override string GroupName => "launchpad.options.votingType";

    public override Func<bool> GroupVisible => CustomGameModeManager.IsClassic;

    [ModdedEnumOption("launchpad.options.votingType.votingType", typeof(VotingTypes))]
    public VotingTypes VotingType { get; set; } = VotingTypes.Classic;

    public ModdedNumberOption MaxVotes { get; } = new("launchpad.options.votingType.maxVotes", 3, 2, 5, 1, MiraNumberSuffixes.None)
    {
        Visible = VotingTypesManager.CanVoteMultiple
    };

    public ModdedToggleOption AllowVotingForSamePerson { get; } = new("launchpad.options.votingType.allowMultipleVotesOnSamePlayer", false)
    {
        Visible = VotingTypesManager.CanVoteMultiple
    };

    public ModdedToggleOption AllowConfirmingVotes { get; } = new("launchpad.options.votingType.allowConfirmingVotes", false)
    {
        Visible = () => !VotingTypesManager.CanVoteMultiple()
    };

    public ModdedToggleOption HideVotingIcons { get; } = new("launchpad.options.votingType.hideVotingIcons", false)
    {
        Visible = () => VotingTypesManager.UseChance() || OptionGroupSingleton<VotingOptions>.Instance.ShowPercentages.Value
    };

    public ModdedToggleOption ShowPercentages { get; } = new("launchpad.options.votingType.showPercentages", false)
    {
        Visible = () => !VotingTypesManager.UseChance()
    };

}