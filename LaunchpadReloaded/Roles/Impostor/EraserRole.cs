using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Ported from TOHO (Eraser). In a meeting, the Eraser's vote erases a player's role instead of counting as a vote.
/// The voting and the role change live in EraserEvents.
/// </summary>
public class EraserRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    /// <summary>Erases used per Eraser. Every client sees the same votes, so every client keeps the same count.</summary>
    public static readonly Dictionary<byte, int> Used = [];

    /// <summary>Players whose role gets erased when the current meeting ends.</summary>
    public static readonly HashSet<byte> Pending = [];

    /// <summary>Erasers that already used their erase in the current meeting (one per meeting, like TOHO).</summary>
    public static readonly HashSet<byte> UsedThisMeeting = [];

    public Color RoleColor => LaunchpadPalette.EraserColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.ImpostorSupport;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.HideButton,
    };
}
