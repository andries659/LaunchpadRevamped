using LaunchpadRevamped.Networking;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Meeting.Voting;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers.Game.Neutral;

/// <summary>
/// Ported from TOHO (Evader). When the vote ends on you, there is a chance nobody gets ejected.
/// </summary>
public sealed class EvaderModifier : ExtraModifier
{
    protected override string Key => "evader";
    protected override string DisplayName => "Evader";

    protected override string Description =>
        $"When you are voted out, there is a {OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.EvaderEscapeChance:0}% chance you are not ejected.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.EvaderChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsNeutral(role);

    [RegisterEvent]
    public static void OnProcessVotes(ProcessVotesEvent @event)
    {
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        var exiled = @event.ExiledPlayer;
        if (exiled == null || !exiled.Object || !exiled.Object.HasModifier<EvaderModifier>())
        {
            return;
        }

        if (UnityEngine.Random.value * 100f >= OptionGroupSingleton<ExtraNeutralModifierOptions>.Instance.EvaderEscapeChance)
        {
            return;
        }

        @event.ExiledPlayer = null;
        RpcWelcomeMessage.Broadcast($"{exiled.PlayerName} evaded the vote and was not ejected!");
    }
}
