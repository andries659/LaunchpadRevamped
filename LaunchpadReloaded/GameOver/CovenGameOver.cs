using System.Linq;
using LaunchpadRevamped.Features;
using LaunchpadRevamped.Roles.Coven;
using MiraAPI.GameEnd;
using MiraAPI.Translation;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.GameOver;

public sealed class CovenGameOver : CustomGameOver
{
    public override bool VerifyCondition(PlayerControl playerControl, NetworkedPlayerInfo[] winners)
    {
        return winners.Length > 0 && winners.All(winner => winner.Role is ICovenRole);
    }

    public override void AfterEndGameSetup(EndGameManager endGameManager)
    {
        endGameManager.WinText.text = MiraLocaleManager.Get("launchpad.gameover.coven.win");
        endGameManager.WinText.color = LaunchpadPalette.CovenColor;
        endGameManager.BackgroundBar.material.SetColor(ShaderID.Color, LaunchpadPalette.CovenColor);
    }
}
