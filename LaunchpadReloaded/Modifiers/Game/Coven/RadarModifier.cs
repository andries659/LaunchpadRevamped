using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options.Modifiers;
using LaunchpadRevamped.Utilities;
using MiraAPI.GameOptions;
using UnityEngine;

namespace LaunchpadRevamped.Modifiers.Game.Coven;

/// <summary>
/// Ported from TOHO (Radar). An arrow always points at the closest living player. Only the owner sees it.
/// </summary>
public sealed class RadarModifier : ExtraModifier
{
    protected override string Key => "radar";
    protected override string DisplayName => "Radar";
    protected override string Description => "An arrow always points at the closest living player.";

    public override int GetAssignmentChance() => (int)OptionGroupSingleton<ExtraCovenModifierOptions>.Instance.RadarChance;

    public override bool IsModifierValidOn(RoleBehaviour role) => base.IsModifierValidOn(role) && RoleTeamChecks.IsCoven(role);

    private ArrowBehaviour? _arrow;

    public override void FixedUpdate()
    {
        if (!Player || !Player.AmOwner || !Player.Data)
        {
            return;
        }

        if (Player.Data.IsDead || MeetingHud.Instance || !ShipStatus.Instance)
        {
            HideArrow();
            return;
        }

        PlayerControl? closest = null;
        var bestDistance = float.MaxValue;
        foreach (var other in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!other || other == Player || !other.Data || other.Data.IsDead || other.Data.Disconnected)
            {
                continue;
            }

            var distance = Vector2.Distance(Player.GetTruePosition(), other.GetTruePosition());
            if (distance < bestDistance)
            {
                bestDistance = distance;
                closest = other;
            }
        }

        if (!closest)
        {
            HideArrow();
            return;
        }

        if (!_arrow)
        {
            _arrow = MiraAPI.Utilities.Helpers.CreateArrow(Player.transform, LaunchpadPalette.CovenColor);
        }

        _arrow!.gameObject.SetActive(true);
        _arrow.target = closest!.transform.position;
    }

    private void HideArrow()
    {
        if (_arrow)
        {
            _arrow!.gameObject.SetActive(false);
        }
    }

    public override void OnDeactivate()
    {
        if (_arrow)
        {
            Object.Destroy(_arrow!.gameObject);
            _arrow = null;
        }
    }
}
