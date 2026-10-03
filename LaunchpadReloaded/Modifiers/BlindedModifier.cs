using LaunchpadRevamped.Options.Roles.Impostor;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.Translation;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// Applied locally to players caught in a Grenadier's flash. White-out that fades away,
/// plus reduced vision (see BlindedPatches) for the duration.
/// </summary>
public sealed class BlindedModifier : BaseModifier
{
    // Fraction of the duration the screen stays fully white before it starts fading.
    private const float SolidPortion = 0.4f;

    private float _elapsed;
    private float _duration;

    public override string ModifierName => MiraLocaleManager.Get("launchpad.modifier.blinded");
    public override bool HideOnUi => true;

    public override void OnActivate()
    {
        _elapsed = 0f;
        _duration = Mathf.Max(0.5f, OptionGroupSingleton<GrenadierOptions>.Instance.BlindDuration);

        if (!Player.AmOwner)
        {
            return;
        }

        SetOverlay(1f);
    }

    public override void FixedUpdate()
    {
        if (!Player.AmOwner)
        {
            return;
        }

        // The effect never carries into meetings or past death.
        if (MeetingHud.Instance || Player.Data.IsDead)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        _elapsed += Time.fixedDeltaTime;
        if (_elapsed >= _duration)
        {
            ModifierComponent!.RemoveModifier(this);
            return;
        }

        var progress = _elapsed / _duration;
        var alpha = progress < SolidPortion ? 1f : Mathf.Lerp(1f, 0f, (progress - SolidPortion) / (1f - SolidPortion));
        SetOverlay(alpha);
    }

    public override void OnDeactivate()
    {
        if (!Player.AmOwner || !HudManager.InstanceExists)
        {
            return;
        }

        HudManager.Instance.FullScreen.gameObject.SetActive(false);
    }

    public override void OnDeath(DeathReason reason)
    {
        ModifierComponent!.RemoveModifier(this);
    }

    private static void SetOverlay(float alpha)
    {
        if (!HudManager.InstanceExists)
        {
            return;
        }

        var fullScreen = HudManager.Instance.FullScreen;
        fullScreen.gameObject.SetActive(true);
        fullScreen.color = new Color(1f, 1f, 1f, alpha);
    }
}
