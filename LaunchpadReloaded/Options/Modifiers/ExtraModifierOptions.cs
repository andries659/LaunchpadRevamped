using LaunchpadRevamped.Features;
using MiraAPI.GameOptions;
using MiraAPI.GameOptions.Attributes;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Options.Modifiers;

// Chances and settings of the modifiers ported from TOHO. Titles are plain English; add them to the locale files to translate.

public class ExtraUniversalModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "More Universal Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 6;

    [ModdedNumberOption("Fragile Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float FragileChance { get; set; } = 0f;
}

public class ExtraImpostorModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "More Impostor Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 7;
    public override Color GroupColor => Palette.ImpostorRoleHeaderRed;

    [ModdedNumberOption("Mare Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float MareChance { get; set; } = 0f;

    [ModdedNumberOption("Mare Extra Speed In Lights Out", 0.1f, 0.5f, 0.1f, suffixType: MiraNumberSuffixes.None)]
    public float MareSpeedBoost { get; set; } = 0.3f;

    [ModdedNumberOption("Mare Kill Cooldown In Lights Out", 0f, 60f, 2.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float MareKillCooldown { get; set; } = 7.5f;

    [ModdedNumberOption("Temporal Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float TemporalChance { get; set; } = 0f;

    [ModdedNumberOption("Temporal Button Cooldown Increase Per Kill", 1f, 10f, 1f, suffixType: MiraNumberSuffixes.Seconds)]
    public float TemporalIncrease { get; set; } = 5f;

    [ModdedNumberOption("Swift Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SwiftChance { get; set; } = 0f;

    [ModdedNumberOption("Clumsy Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float ClumsyChance { get; set; } = 0f;

    [ModdedNumberOption("Clumsy Chance To Miss", 0f, 100f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float ClumsyMissChance { get; set; } = 50f;

    [ModdedToggleOption("Last Impostor Is Given Automatically")]
    public bool LastImpostorEnabled { get; set; } = false;

    [ModdedNumberOption("Last Impostor Kill Cooldown Reduction", 5f, 95f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float LastImpostorReduction { get; set; } = 50f;

    [ModdedNumberOption("Stealer Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float StealerChance { get; set; } = 0f;

    [ModdedNumberOption("Stealer Extra Votes Per Kill", 0.5f, 5f, 0.5f, suffixType: MiraNumberSuffixes.None)]
    public float StealerVotesPerKill { get; set; } = 1f;

    [ModdedNumberOption("Quota Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float QuotaChance { get; set; } = 0f;

    [ModdedNumberOption("Quota Kills Needed To Win", 1f, 5f, 1f, suffixType: MiraNumberSuffixes.None)]
    public float QuotaKills { get; set; } = 3f;

    [ModdedNumberOption("Lag Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float LagChance { get; set; } = 0f;

    [ModdedNumberOption("Lag Kill Delay", 0.5f, 3f, 0.5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float LagDelay { get; set; } = 1f;

    [ModdedNumberOption("Circumvent Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float CircumventChance { get; set; } = 0f;
}

public class ExtraNeutralModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "More Neutral Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 8;
    public override Color GroupColor => Color.gray;

    [ModdedNumberOption("Lucky Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float LuckyChance { get; set; } = 0f;

    [ModdedNumberOption("Lucky Chance To Dodge A Kill", 0f, 100f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float LuckyDodgeChance { get; set; } = 25f;

    [ModdedNumberOption("Extra Life Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float ExtraLifeChance { get; set; } = 0f;

    [ModdedNumberOption("Evader Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float EvaderChance { get; set; } = 0f;

    [ModdedNumberOption("Evader Chance To Escape Ejection", 0f, 100f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float EvaderEscapeChance { get; set; } = 30f;

    [ModdedNumberOption("Overclocked Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float OverclockedChance { get; set; } = 0f;

    [ModdedNumberOption("Overclocked Kill Cooldown Reduction", 5f, 75f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float OverclockedReduction { get; set; } = 25f;

    [ModdedNumberOption("Underclocked Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float UnderclockedChance { get; set; } = 0f;

    [ModdedNumberOption("Underclocked Kill Cooldown Increase", 5f, 100f, 5f, suffixType: MiraNumberSuffixes.Percent)]
    public float UnderclockedIncrease { get; set; } = 25f;

    [ModdedNumberOption("Seer Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SeerChance { get; set; } = 0f;

    [ModdedNumberOption("Watcher Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float WatcherChance { get; set; } = 0f;

    [ModdedNumberOption("Subversion Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SubversionChance { get; set; } = 0f;

    [ModdedNumberOption("Chronos Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float ChronosChance { get; set; } = 0f;

    [ModdedNumberOption("Chronos Discussion Cut When Chronos Calls", 10f, 90f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float ChronosCut { get; set; } = 50f;

    [ModdedNumberOption("Chronos Discussion Added When Someone Else Calls", 5f, 60f, 5f, suffixType: MiraNumberSuffixes.Seconds)]
    public float ChronosExtend { get; set; } = 15f;
}

public class ExtraCovenModifierOptions : AbstractOptionGroup
{
    public override string GroupName => "More Coven Modifiers";
    public override MenuCategory ParentMenu => MenuCategory.Modifiers;
    public override uint GroupPriority => 9;
    public override Color GroupColor => LaunchpadPalette.CovenColor;

    [ModdedNumberOption("Necroview Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float NecroviewChance { get; set; } = 0f;

    [ModdedNumberOption("Loyal Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float LoyalChance { get; set; } = 0f;

    [ModdedNumberOption("Stubborn Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float StubbornChance { get; set; } = 0f;

    [ModdedNumberOption("Saboteur Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float SaboteurChance { get; set; } = 0f;

    [ModdedNumberOption("Eavesdropper Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float EavesdropperChance { get; set; } = 0f;

    [ModdedNumberOption("Eavesdropper Chance To Learn A Role", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float EavesdropperRevealChance { get; set; } = 50f;

    [ModdedNumberOption("Radar Chance", 0f, 100f, 10f, suffixType: MiraNumberSuffixes.Percent)]
    public float RadarChance { get; set; } = 0f;
}
