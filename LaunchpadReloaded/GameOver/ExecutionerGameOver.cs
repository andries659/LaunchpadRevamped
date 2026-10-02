using LaunchpadRevamped.Features;
using MiraAPI.Translation;
using LaunchpadRevamped.Roles.Neutral;
using MiraAPI.GameEnd;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.GameOver;

public sealed class ExecutionerGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners is [{ Role: ExecutionerRole }];
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = $"<size=80%>{MiraLocaleManager.Get("launchpad.gameover.executioner.win")}</size>";
        endGameManager.WinText.color = LaunchpadPalette.ExecutionerColor;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, LaunchpadPalette.ExecutionerColor);
    }
}