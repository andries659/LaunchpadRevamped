using System;
using System.Collections.Generic;
using LaunchpadRevamped.Roles.Coven;
using LaunchpadRevamped.Roles.Crewmate;
using LaunchpadRevamped.Roles.Impostor;
using LaunchpadRevamped.Roles.Neutral;

namespace LaunchpadRevamped.Features;

/// <summary>
/// Who came up with a role and who coded it. Shown under the description on the settings page of the role.
/// Either name can be left null (that line is then simply not shown); a role with neither shows nothing.
/// </summary>
public readonly record struct RoleCredit(string? Idea, string? Coder)
{
    public bool IsEmpty => string.IsNullOrWhiteSpace(Idea) && string.IsNullOrWhiteSpace(Coder);

    /// <summary>The text shown in the menu, one credit per line.</summary>
    public string ToMenuText()
    {
        var lines = new List<string>(2);
        if (!string.IsNullOrWhiteSpace(Idea))
        {
            lines.Add($"<b>★ Role Idea by {Idea}</b>");
        }

        if (!string.IsNullOrWhiteSpace(Coder))
        {
            lines.Add($"<b>★ Coded by {Coder}</b>");
        }

        return string.Join("\n", lines);
    }
}

public static class RoleCredits
{
    private const string XtraCube = "XtraCube";
    private const string Angxl = "angxlwtf";
    private const string Andries = "<color=#3293a8>Andries</color>";
    private const string Farhan = "<color=#ff1f1f>Farhan</color>";

    // "Coded by" is filled in from who first added each role's file in the git history, so check it, and fill in
    // the "Role Idea by" names yourself (null = not shown). Roles missing from this table show no credits.
    private static readonly Dictionary<Type, RoleCredit> Credits = new()
    {
        // Crewmate
        [typeof(CaptainRole)] = new(null, XtraCube),
        [typeof(CoronerRole)] = new(null, Angxl),
        [typeof(GamblerRole)] = new(null, Angxl),
        [typeof(LpDetectiveRole)] = new(null, XtraCube),
        [typeof(LuckyRole)] = new(Andries, Andries),
        [typeof(MedicRole)] = new(null, Angxl),
        [typeof(SealerRole)] = new(null, Angxl),
        [typeof(SheriffRole)] = new(null, Angxl),
        [typeof(SnitchRole)] = new(Andries, Andries),
        [typeof(TimeRewinderRole)] = new(Andries, Andries),

        // Impostor
        [typeof(BomberRole)] = new(Andries, Andries),
        [typeof(BurrowerRole)] = new(null, Angxl),
        [typeof(EsperRole)] = new(Andries, Andries),
        [typeof(GrenadierRole)] = new(Andries, Andries),
        [typeof(HackerRole)] = new(null, Angxl),
        [typeof(HitmanRole)] = new(null, Angxl),
        [typeof(JanitorRole)] = new(null, XtraCube),
        [typeof(PoltergeistRole)] = new(Andries, Andries),
        [typeof(SurgeonRole)] = new(null, Angxl),
        [typeof(SwapshifterRole)] = new(null, Angxl),
        [typeof(InfiltratorRole)] = new(Farhan, Andries),

        // Neutral
        [typeof(ArsonistRole)] = new(Andries, Andries),
        [typeof(ExecutionerRole)] = new(null, Angxl),
        [typeof(JesterRole)] = new(null, Angxl),
        [typeof(NeutralGhostRole)] = new(null, Angxl),
        [typeof(ReaperRole)] = new(null, Angxl),
        [typeof(SurvivorRole)] = new(Andries, Andries),

        // Coven
        [typeof(CovenLeaderRole)] = new(Andries, Andries),
        [typeof(HexMasterRole)] = new(Andries, Andries),
        [typeof(MedusaRole)] = new(Andries, Andries),
        [typeof(NecromancerRole)] = new(Andries, Andries),
        [typeof(PoisonerRole)] = new(Andries, Andries),
    };

    public static bool TryGet(Type roleType, out RoleCredit credit)
    {
        return Credits.TryGetValue(roleType, out credit) && !credit.IsEmpty;
    }
}
