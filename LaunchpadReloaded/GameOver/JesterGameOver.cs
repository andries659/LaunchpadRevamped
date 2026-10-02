using LaunchpadRevamped.Features;
using MiraAPI.Translation;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.GameOver;

public sealed class JesterGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners is [{ Role: JesterRole }];
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = MiraLocaleManager.Get("launchpad.gameover.jester.win");
        endGameManager.WinText.color = LaunchpadPalette.JesterColor;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, LaunchpadPalette.JesterColor);
    }
}