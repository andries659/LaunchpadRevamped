using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.Translation;
using LaunchpadRevamped.Options.Modifiers.Crewmate;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MiraAPI.Utilities.Assets;
using UnityEngine;
using LaunchpadRevamped.Features;

namespace LaunchpadRevamped.Modifiers.Game.Crewmate;

public sealed class MayorModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.mayor");
    public override string GetDescription() => MiraLocaleManager.GetParsed(
        "launchpad.modifier.mayor.TabDescription",
        new[] { ("{votes}", OptionGroupSingleton<MayorOptions>.Instance.ExtraVotes.ToString()) });
    public override LoadableAsset<Sprite>? ModifierIcon => LaunchpadAssets.MayorIcon;

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.MayorChance;
    public override int GetAmountPerGame() => 1;

    public override void OnMeetingStart()
    {
        var voteData = Player.GetVoteData();
        if (!voteData) return;

        voteData.VotesRemaining += (int)OptionGroupSingleton<MayorOptions>.Instance.ExtraVotes;
    }
}
