using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Features;

public static class LaunchpadAssets
{
    public static readonly AssetBundle Bundle = AssetBundleManager.Load("launchpad-assets");
    public static readonly AssetBundle Wiki = AssetBundleManager.Load("wiki");

    // Shaders
    public static readonly LoadableAsset<Shader> BloomShader = new LoadableBundleAsset<Shader>("Bloom.shader", Bundle);

    // Materials
    public static readonly LoadableAsset<Material> GradientMaterial = new LoadableBundleAsset<Material>("GradientPlayerMaterial", Bundle);
    public static readonly LoadableAsset<Material> MaskedGradientMaterial = new LoadableBundleAsset<Material>("MaskedGradientMaterial", Bundle);

    // Sprites
    public static readonly LoadableAsset<Sprite> BlankButton = new LoadableBundleAsset<Sprite>("BlankButton", Bundle);
    public static readonly LoadableAsset<Sprite> CallButton = new LoadableBundleAsset<Sprite>("CallMeeting.png", Bundle);
    public static readonly LoadableAsset<Sprite> DissectButton = new LoadableBundleAsset<Sprite>("Dissect.png", Bundle);
    public static readonly LoadableAsset<Sprite> DragButton = new LoadableBundleAsset<Sprite>("Drag.png", Bundle);
    public static readonly LoadableAsset<Sprite> DropButton = new LoadableBundleAsset<Sprite>("Drop.png", Bundle);
    public static readonly LoadableAsset<Sprite> HackButton = new LoadableBundleAsset<Sprite>("Hack.png", Bundle);
    public static readonly LoadableAsset<Sprite> HideButton = new LoadableBundleAsset<Sprite>("Clean.png", Bundle);
    public static readonly LoadableAsset<Sprite> InjectButton = new LoadableBundleAsset<Sprite>("Inject.png", Bundle);
    public static readonly LoadableAsset<Sprite> InstinctButton = new LoadableBundleAsset<Sprite>("Instinct.png", Bundle);
    public static readonly LoadableAsset<Sprite> InvestigateButton = new LoadableBundleAsset<Sprite>("Investigate.png", Bundle);
    public static readonly LoadableAsset<Sprite> DigVentButton = new LoadableBundleAsset<Sprite>("DigVent.png", Bundle);
    public static readonly LoadableAsset<Sprite> JesterIcon = new LoadableBundleAsset<Sprite>("Jester.png", Bundle);
    public static readonly LoadableAsset<Sprite> ReviveButton = new LoadableBundleAsset<Sprite>("Revive.png", Bundle);
    public static readonly LoadableAsset<Sprite> ShootButton = new LoadableBundleAsset<Sprite>("Shoot.png", Bundle);
    public static readonly LoadableAsset<Sprite> ZoomButton = new LoadableBundleAsset<Sprite>("Zoom.png", Bundle);
    public static readonly LoadableAsset<Sprite> SealButton = new LoadableBundleAsset<Sprite>("SealVent.png", Bundle);
    public static readonly LoadableAsset<Sprite> SwapButton = new LoadableBundleAsset<Sprite>("Swap.png", Bundle);
    public static readonly LoadableAsset<Sprite> FreezeButton = new LoadableBundleAsset<Sprite>("Freeze.png", Bundle);
    public static readonly LoadableAsset<Sprite> SoulButton = new LoadableBundleAsset<Sprite>("StealSoul.png", Bundle);
    public static readonly LoadableAsset<Sprite> GambleButton = new LoadableBundleAsset<Sprite>("Gamble.png", Bundle);
    public static readonly LoadableAsset<Sprite> DeadlockButton = new LoadableBundleAsset<Sprite>("Deadlock.png", Bundle);

    // Embedded PNGs (Resources/**/*.png are embedded by the csproj, so no asset bundle rebuild needed)
    public static readonly LoadableAsset<Sprite> DouseButton = new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Douse.png");
    public static readonly LoadableAsset<Sprite> BurnButton = new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Burn.png");
    public static readonly LoadableAsset<Sprite> BlindButton = new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Blind.png");
    public static readonly LoadableAsset<Sprite> BombButton = new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Bomb.png");

    // Role icons
    public static readonly LoadableAsset<Sprite> ArsonistIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.Arsonist.png");
    public static readonly LoadableAsset<Sprite> BomberIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.Bomber.png");
    public static readonly LoadableAsset<Sprite> GrenadierIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.Grenadier.png");
    public static readonly LoadableAsset<Sprite> LuckyIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.Lucky.png");
    public static readonly LoadableAsset<Sprite> SnitchIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.Snitch.png");
    public static readonly LoadableAsset<Sprite> TimeRewinderIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.Icons.TimeRewinder.png");
    public static readonly LoadableAsset<Sprite> RewindButton = new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Rewind.png");

    // Role category art for the role settings tab (from TOHO)
    public static readonly LoadableAsset<Sprite> CategoryIconCrewmate = new LoadableResourceAsset("LaunchpadRevamped.Resources.TabIcon_CrewmateRoles.png", 120f);
    public static readonly LoadableAsset<Sprite> CategoryIconImpostor = new LoadableResourceAsset("LaunchpadRevamped.Resources.TabIcon_ImpostorRoles.png", 120f);
    public static readonly LoadableAsset<Sprite> CategoryIconNeutral = new LoadableResourceAsset("LaunchpadRevamped.Resources.TabIcon_NeutralRoles.png", 120f);
    public static readonly LoadableAsset<Sprite> CovenIcon = new LoadableResourceAsset("LaunchpadRevamped.Resources.TabIcon_CovenRoles.png", 120f);
    public static readonly LoadableAsset<Sprite> CategoryIconModifiers = new LoadableResourceAsset("LaunchpadRevamped.Resources.TabIcon_Modifiers.png", 120f);
    public static readonly LoadableAsset<Sprite> CategoryBannerCrewmate = new LoadableResourceAsset("LaunchpadRevamped.Resources.CrewmateImage.png");
    public static readonly LoadableAsset<Sprite> CategoryBannerImpostor = new LoadableResourceAsset("LaunchpadRevamped.Resources.ImpostorImage.png");
    public static readonly LoadableAsset<Sprite> CategoryBannerNeutral = new LoadableResourceAsset("LaunchpadRevamped.Resources.NeutralImage.png");
    public static readonly LoadableAsset<Sprite> CategoryBannerCoven = new LoadableResourceAsset("LaunchpadRevamped.Resources.CovenImage.png");
    public static readonly LoadableAsset<Sprite> CategoryBannerModifiers = new LoadableResourceAsset("LaunchpadRevamped.Resources.ModifierImage.png");
    public static readonly LoadableAsset<Sprite> CategoryTextCrewmate = new LoadableResourceAsset("LaunchpadRevamped.Resources.CrewmatesText.png", 50f);
    public static readonly LoadableAsset<Sprite> CategoryTextImpostor = new LoadableResourceAsset("LaunchpadRevamped.Resources.ImpostorsText.png", 50f);
    public static readonly LoadableAsset<Sprite> CategoryTextNeutral = new LoadableResourceAsset("LaunchpadRevamped.Resources.NeutralsText.png", 50f);
    public static readonly LoadableAsset<Sprite> CategoryTextCoven = new LoadableResourceAsset("LaunchpadRevamped.Resources.CovenText.png", 50f);
    public static readonly LoadableAsset<Sprite> CategoryTextModifiers = new LoadableResourceAsset("LaunchpadRevamped.Resources.ModifiersText.png", 50f);

    // Placeholders for the Coven, Survivor and Poltergeist until dedicated sprites exist. To swap one in, drop the PNG into
    // Resources/Buttons or Resources/Icons and replace the line, e.g.
    // new LoadableResourceAsset("LaunchpadRevamped.Resources.Buttons.Smite.png")
    public static readonly LoadableAsset<Sprite> SmiteButton = ShootButton;
    public static readonly LoadableAsset<Sprite> PoisonButton = InjectButton;
    public static readonly LoadableAsset<Sprite> GazeButton = FreezeButton;
    public static readonly LoadableAsset<Sprite> ReanimateButton = ReviveButton;
    public static readonly LoadableAsset<Sprite> HexButton = SealButton;
    public static readonly LoadableAsset<Sprite> VestButton = HideButton;
    public static readonly LoadableAsset<Sprite> PoltergeistIcon = DragButton;
    public static readonly LoadableAsset<Sprite> SurvivorIcon = HideButton;

    public static readonly LoadableAsset<Sprite> NotepadSprite = new LoadableBundleAsset<Sprite>("NotepadButton.png", Bundle);
    public static readonly LoadableAsset<Sprite> NotepadActiveSprite = new LoadableBundleAsset<Sprite>("NotepadButtonActive.png", Bundle);

    // Banner Sprites
    public static readonly LoadableAsset<Sprite> CaptainBanner = new LoadableBundleAsset<Sprite>("CaptainBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> DetectiveBanner = new LoadableBundleAsset<Sprite>("DetectiveBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> HackerBanner = new LoadableBundleAsset<Sprite>("HackerBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> JanitorBanner = new LoadableBundleAsset<Sprite>("JanitorBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> JesterBanner = new LoadableBundleAsset<Sprite>("JesterBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> MedicBanner = new LoadableBundleAsset<Sprite>("MedicBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> SheriffBanner = new LoadableBundleAsset<Sprite>("SheriffBanner.png", Bundle);
    public static readonly LoadableAsset<Sprite> SurgeonBanner = new LoadableBundleAsset<Sprite>("SurgeonBanner.png", Bundle);

    // Object Sprites
    public static readonly LoadableAsset<Sprite> Bone = new LoadableBundleAsset<Sprite>("Bone.png", Bundle);
    public static readonly LoadableAsset<Sprite> Footstep = new LoadableBundleAsset<Sprite>("Footstep.png", Bundle);
    public static readonly LoadableAsset<Sprite> VentTape = new LoadableBundleAsset<Sprite>("VentTape.png", Bundle);
    public static readonly LoadableAsset<Sprite> VentTapePolus = new LoadableBundleAsset<Sprite>("VentTapePolus.png", Bundle);
    public static readonly LoadableAsset<Sprite> KnifeHandSprite = new LoadableBundleAsset<Sprite>("KnifeHand.png", Bundle);
    public static readonly LoadableAsset<Sprite> NodeSprite = new LoadableBundleAsset<Sprite>("Node.png", Bundle);
    
    public static readonly LoadableAsset<Sprite> DeadlockHonor = new LoadableBundleAsset<Sprite>("DeadlockHonor.png", Bundle);
    public static readonly LoadableAsset<Sprite> DeadlockTarget = new LoadableBundleAsset<Sprite>("DeadlockTarget.png", Bundle);
    public static readonly LoadableAsset<Sprite> DeadlockVignette = new LoadableBundleAsset<Sprite>("DeadlockVignette.png", Bundle);
    public static readonly LoadableAsset<Sprite> FrozenBodyOverlay = new LoadableBundleAsset<Sprite>("BodyFrozenOverlay.png", Bundle);

    // Sounds
    public static readonly LoadableAsset<AudioClip> BeepSound = new LoadableBundleAsset<AudioClip>("Beep.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> InjectSound = new LoadableBundleAsset<AudioClip>("Inject.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> DissectSound = new LoadableBundleAsset<AudioClip>("Dissect.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> HackingSound = new LoadableBundleAsset<AudioClip>("HackAmbience.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> PingSound = new LoadableBundleAsset<AudioClip>("Ping.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> MoneySound = new LoadableBundleAsset<AudioClip>("MoneySound.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> BuzzerSound = new LoadableBundleAsset<AudioClip>("Buzzer.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> DigSound = new LoadableBundleAsset<AudioClip>("Dig.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> TapeSound = new LoadableBundleAsset<AudioClip>("Tape.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> SwooshSound = new LoadableBundleAsset<AudioClip>("Swoosh.mp3", Bundle);
    public static readonly LoadableAsset<AudioClip> ReaperSound = new LoadableBundleAsset<AudioClip>("CollectSoul.mp3", Bundle);

    // Deadlock Sounds
    public static readonly LoadableAsset<AudioClip> DeadlockAmbience = new LoadableBundleAsset<AudioClip>("DeadlockAmbience.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockFadeIn = new LoadableBundleAsset<AudioClip>("DeadlockFadeIn.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockFadeOut = new LoadableBundleAsset<AudioClip>("DeadlockFadeOut.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockClockLeft = new LoadableBundleAsset<AudioClip>("DeadlockClockLeft.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockClockRight = new LoadableBundleAsset<AudioClip>("DeadlockClockRight.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockMark = new LoadableBundleAsset<AudioClip>("DeadlockMark.wav", Bundle);
    public static readonly LoadableAsset<AudioClip> DeadlockKillConfirmal = new LoadableBundleAsset<AudioClip>("DeadlockKillConfirmal.wav", Bundle);

    // Other
    public static readonly LoadableAsset<GameObject> DetectiveGame = new LoadableBundleAsset<GameObject>("JournalMinigame", Bundle);
    public static readonly LoadableAsset<GameObject> RoleMinigame = new LoadableBundleAsset<GameObject>("RoleGuessingMinigame", Bundle);
    public static readonly LoadableAsset<GameObject> ReaperDisplay = new LoadableBundleAsset<GameObject>("ReaperSoulDisplay", Bundle);
    public static readonly LoadableAsset<GameObject> Notepad = new LoadableBundleAsset<GameObject>("Notepad", Bundle);
    public static readonly LoadableAsset<GameObject> NodeGame = new LoadableBundleAsset<GameObject>("NodeMinigame", Bundle);

    public static readonly LoadableAsset<GameObject> PlayerTags = new LoadableBundleAsset<GameObject>("PlayerTags", Bundle);
}
