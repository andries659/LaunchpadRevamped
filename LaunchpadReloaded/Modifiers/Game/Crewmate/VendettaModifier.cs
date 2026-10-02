using LaunchpadRevamped.Buttons.Modifiers;
using MiraAPI.Translation;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Options.Modifiers.Crewmate;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using MiraAPI.GameOptions;
using MiraAPI.Hud;
using MiraAPI.Modifiers;
using MiraAPI.Networking;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;

namespace LaunchpadRevamped.Modifiers.Game.Crewmate;

[MiraIgnore]
public sealed class VendettaModifier : LPModifier
{
    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.vendetta");
    public override string GetDescription()
    {
        var uses = OptionGroupSingleton<VendettaOptions>.Instance.MarkUses;
        return MiraLocaleManager.GetParsed("launchpad.modifier.vendetta.TabDescription", new()
        {
            ["{uses}"] = uses.ToString(),
            ["{unit}"] = MiraLocaleManager.Get(uses > 1 ? "launchpad.modifier.vendetta.unit.other" : "launchpad.modifier.vendetta.unit.one"),
        });
    }

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.VendettaChance;
    public override int GetAmountPerGame() => 1;

    public override void OnActivate()
    {
        if (!Player.AmOwner) return;

        CustomButtonSingleton<VendettaMarkButton>.Instance.Button?.Show();
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner) return;

        CustomButtonSingleton<VendettaMarkButton>.Instance.Button?.Hide();
    }

    [RegisterEvent]
    public static void RoundStartEvent(RoundStartEvent @event)
    {
        if (@event.TriggeredByIntro) return;

        if (PlayerControl.LocalPlayer.HasModifier<VendettaModifier>())
        {
            CustomButtonSingleton<VendettaMarkButton>.Instance.SetUses(CustomButtonSingleton<VendettaMarkButton>.Instance.MaxUses);
        }

        if (!PlayerControl.LocalPlayer.IsHost()) return;

        var victims = ModifierUtils.GetActiveModifiers<VendettaMarkModifier>();
        foreach (var mod in victims)
        {
            mod.Player.RpcRemoveModifier<VendettaMarkModifier>();
            var voteData = mod.Player.GetVoteData();
            if (voteData && voteData.VotedFor(mod.Vendetta.PlayerId))
            {
                mod.Vendetta.RpcCustomMurder(mod.Player, teleportMurderer: false);
            }
        }
    }
}