using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.GameOver;

public sealed class ArsonistGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners is [{ Role: ArsonistRole }];
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = MiraLocaleManager.Get("launchpad.gameover.arsonist.win");
        endGameManager.WinText.color = LaunchpadPalette.ArsonistColor;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, LaunchpadPalette.ArsonistColor);
    }
}
