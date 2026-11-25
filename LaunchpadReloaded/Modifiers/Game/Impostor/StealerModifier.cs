using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Impostor;

/// <summary>
/// Ported from TOHO (Stealer). Every kill earns extra votes for the next meetings, added the same way the Mayor
/// modifier adds its votes. Kills are counted on every client, so everyone agrees on the number.
/// </summary>
public sealed class StealerModifier : ExtraModifier
{
    protected override string Key => "stealer";
    protected override string DisplayName => "Stealer";

    protected override string Description =>
        $"Every kill gives you {OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.StealerVotesPerKill:0.#} extra vote(s) in meetings (rounded down).";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.StealerChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsImpostorTeam(role);

    public int Kills { get; private set; }

    [RegisterEvent]
    public static void OnAfterMurder(AfterMurderEvent @event)
    {
        var stealer = @event.Source ? @event.Source.GetModifier<StealerModifier>() : null;
        if (stealer != null)
        {
            stealer.Kills++;
        }
    }

    public override void OnMeetingStart()
    {
        var extra = Mathf.FloorToInt(Kills * OptionGroupSingleton<ExtraImpostorModifierOptions>.Instance.StealerVotesPerKill);
        if (extra <= 0)
        {
            return;
        }

        var voteData = Player.GetVoteData();
        if (!voteData)
        {
            return;
        }

        voteData.VotesRemaining += extra;
    }
}
