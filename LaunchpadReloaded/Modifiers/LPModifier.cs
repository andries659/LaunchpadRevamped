using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers.Types;
using MiraAPI.PluginLoading;
using System.Linq;
using MiraAPI.Modifiers;

namespace LaunchpadRevamped.Modifiers;

[MiraIgnore]
public abstract class LPModifier : GameModifier
{
    public virtual bool RemoveOnDeath => true;

    /// <summary>Colour used for this modifier in lists such as the /r command. Null means the colour of its team.</summary>
    public virtual UnityEngine.Color? ListColor => null;

    /// <summary>False for modifiers that are never rolled at the start of a game (they are handed out during the game).</summary>
    public virtual bool ListInRoleList => true;

    public override int GetAmountPerGame() => 1;

    public override bool IsModifierValidOn(RoleBehaviour role)
    {
        if (OptionGroupSingleton<LpModifierOptions>.Instance.ModifierLimit == 0) return true;
        return role.Player.GetModifierComponent().ActiveModifiers.OfType<LPModifier>().Count() < OptionGroupSingleton<LpModifierOptions>.Instance.ModifierLimit;
    }

    public override void OnDeath(DeathReason reason)
    {
        if (!RemoveOnDeath) return;
        ModifierComponent!.RemoveModifier(this);
    }
}