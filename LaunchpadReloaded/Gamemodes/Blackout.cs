// EXPERIMENTAL - Blackout. Disabled until it has been tested.
// Written against MiraAPI 0.6.0 (AbstractGameMode). A game mode is registered just by existing, so it stays commented out.
using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using MiraAPI.GameModes;
using UnityEngine;

namespace LaunchpadRevamped.Gamemodes;

/// <summary>
/// The whole map is dark. Crewmates barely see anything, Impostors still see well. Lights can't be fixed or broken.
/// </summary>
public class Blackout : AbstractGameMode
{
    private const float CrewVision = 0.25f;
    private const float ImpostorVision = 1f;

    public override string Name => "Blackout";
    public override string Description => "The lights are out.\n<b><i>Crewmates can barely see. Impostors hunt in the dark.</b></i>";
    public override Color Color => new(0.35f, 0.35f, 0.6f);

    public override bool ShowNormalRoleSettings => false;

    public override void Initialize()
    {
        // Vision is part of the host's options, which are synced to everyone.
        if (!AmongUsClient.Instance.AmHost)
        {
            return;
        }

        if (GameOptionsManager.Instance.CurrentGameOptions.TryCast<NormalGameOptionsV10>() is { } options)
        {
            options.SetFloat(FloatOptionNames.CrewLightMod, CrewVision);
            options.SetFloat(FloatOptionNames.ImpostorLightMod, ImpostorVision);
        }
    }

    public override void HudUpdate(HudManager instance)
    {
        // Nobody can sabotage lights, the blackout is permanent.
        instance.SabotageButton.gameObject.SetActive(false);
    }

    public override bool ShouldShowSabotageMap(MapBehaviour map) => false;
    public override bool CanUseSystemConsole(SystemConsole console) => false;
}
