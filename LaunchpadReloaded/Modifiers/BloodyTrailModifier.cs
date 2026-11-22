using LaunchpadRevamped.Options.Modifiers;
using MiraAPI.GameOptions;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers;

/// <summary>
/// The footprints a killer leaves after killing a player with the Bloody modifier. It is the Detective's footsteps,
/// removed again after <see cref="BloodyOptions.TrailDuration"/> seconds.
/// </summary>
public class BloodyTrailModifier : FootstepsModifier
{
    private float _elapsed;

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        _elapsed += Time.fixedDeltaTime;
        if (_elapsed >= OptionGroupSingleton<BloodyOptions>.Instance.TrailDuration)
        {
            ModifierComponent!.RemoveModifier(this);
        }
    }
}
