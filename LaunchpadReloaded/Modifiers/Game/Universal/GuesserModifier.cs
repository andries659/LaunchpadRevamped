using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Options.Modifiers.Universal;
using MiraAPI.GameOptions;
using MiraAPI.Translation;

namespace LaunchpadRevamped.Modifiers.Game.Universal;

/// <summary>
/// Lets the player guess other players' roles during meetings. A right guess kills the target, a wrong one kills
/// the guesser. The meeting UI lives in GuesserMeetingPatches, the kill logic in GuesserRpc.
/// </summary>
public sealed class GuesserModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.guesser");

    public override string GetDescription() => MiraLocaleManager.GetParsed(
        "launchpad.modifier.guesser.TabDescription",
        new[]
        {
            ("{guesses}", ((int)OptionGroupSingleton<GuesserOptions>.Instance.MaxGuessesPerGame).ToString()),
            ("{perMeeting}", ((int)OptionGroupSingleton<GuesserOptions>.Instance.MaxGuessesPerMeeting).ToString()),
        });

    public override int GetAssignmentChance() =>
        (int)OptionGroupSingleton<UniversalModifierOptions>.Instance.GuesserChance;

    public override int GetAmountPerGame() => 1;

    /// <summary>Guesses used in the whole game. Updated by the guess RPC, so every client agrees.</summary>
    public int GuessesUsed { get; set; }

    /// <summary>Guesses used in the current meeting.</summary>
    public int GuessesThisMeeting { get; set; }

    public bool CanGuessNow
    {
        get
        {
            var options = OptionGroupSingleton<GuesserOptions>.Instance;
            return Player && !Player.Data.IsDead
                   && GuessesUsed < (int)options.MaxGuessesPerGame
                   && GuessesThisMeeting < (int)options.MaxGuessesPerMeeting;
        }
    }

    public override void OnMeetingStart()
    {
        GuessesThisMeeting = 0;
    }
}
