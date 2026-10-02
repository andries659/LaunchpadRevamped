using LaunchpadRevamped.Features;
using MiraAPI.Translation;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.GameOver;

public sealed class ReaperGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners is [{ Role: ReaperRole }];
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = MiraLocaleManager.Get("launchpad.gameover.reaper.win");
        endGameManager.WinText.color = LaunchpadPalette.ReaperColor;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, LaunchpadPalette.ReaperColor);
    }
}