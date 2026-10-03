using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.GameOptions;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Crewmate;

public sealed class ChameleonModifier : LPModifier
{
    private const float FadeSpeed = 4f;
    private const float OwnerHiddenAlpha = 0.3f;

    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.chameleon");
    public override string GetDescription() => MiraLocaleManager.Get("launchpad.modifier.chameleon.TabDescription");
    public override int GetAssignmentChance() => (int)OptionGroupSingleton<CrewmateModifierOptions>.Instance.ChameleonChance;
    public override int GetAmountPerGame() => 1;

    public override void FixedUpdate()
    {
        if (!Player || !Player.cosmetics)
        {
            return;
        }

        // Fully visible while moving. While standing still everyone else sees nothing,
        // and the chameleon only sees a faint outline of themselves.
        var isMoving = Player.MyPhysics.Velocity.magnitude > 0;
        var targetAlpha = isMoving ? 1f : (Player.AmOwner ? OwnerHiddenAlpha : 0f);
        var t = Time.fixedDeltaTime * FadeSpeed;

        var nameText = Player.cosmetics.nameText;
        var nameColor = nameText.color;
        nameText.color = Color.Lerp(nameColor, new Color(nameColor.r, nameColor.g, nameColor.b, targetAlpha), t);

        var body = Player.cosmetics.currentBodySprite.BodySprite;
        body.color = Color.Lerp(body.color, new Color(1f, 1f, 1f, targetAlpha), t);

        foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>())
        {
            cosmetic.color = Color.Lerp(cosmetic.color, new Color(1f, 1f, 1f, targetAlpha), t);
        }
    }

    public override void OnDeactivate()
    {
        if (!Player || !Player.cosmetics)
        {
            return;
        }

        foreach (var cosmetic in Player.cosmetics.transform.GetComponentsInChildren<SpriteRenderer>(true))
        {
            cosmetic.color = Color.white;
        }

        Player.cosmetics.currentBodySprite.BodySprite.color = Color.white;

        // Only restore alpha so role-coloured names (e.g. red for impostors) keep their colour.
        var nameColor = Player.cosmetics.nameText.color;
        Player.cosmetics.nameText.color = new Color(nameColor.r, nameColor.g, nameColor.b, 1f);
    }
}