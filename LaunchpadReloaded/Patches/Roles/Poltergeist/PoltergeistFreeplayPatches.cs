using HarmonyLib;
using LaunchpadRevamped.Roles.Impostor;

namespace LaunchpadRevamped.Patches.Roles.Poltergeist;

/// <summary>
/// Freeplay (practice) has a single Impostor: you. When that Impostor dies, the vanilla end check sees "no Impostors
/// left", shows "Normally The Crew would have just won... For Practice, we revive everyone instead." and revives
/// everyone, which throws the Poltergeist role away the moment it is assigned.
///
/// While a dead Poltergeist exists in freeplay, the end check is skipped so the ghost role can actually be used.
/// Real lobbies are not touched: this only runs when the TutorialManager (freeplay) exists.
/// </summary>
[HarmonyPatch(typeof(LogicGameFlowNormal), nameof(LogicGameFlowNormal.CheckEndCriteria))]
public static class PoltergeistFreeplayPatches
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static bool CheckEndCriteriaPrefix()
    {
        if (!TutorialManager.InstanceExists || !HasDeadPoltergeist())
        {
            return true;
        }

        // Skip the whole check (and with it the practice revive) while someone is playing as a Poltergeist.
        return false;
    }

    private static bool HasDeadPoltergeist()
    {
        foreach (var player in PlayerControl.AllPlayerControls)
        {
            if (player && player.Data && player.Data.IsDead && player.Data.Role is PoltergeistRole)
            {
                return true;
            }
        }

        return false;
    }
}
