using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Ported from TOHO (Vampire). Bites instead of killing: the victim dies after a delay.
/// </summary>
public class VampireRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    /// <summary>
    /// Players this Vampire has bitten and who have not died yet. Only tracked on the Vampire's own client.
    /// </summary>
    public static readonly HashSet<byte> Bitten = [];

    public Color RoleColor => LaunchpadPalette.VampireColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.ImpostorConcealing;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.BiteButton,
        UseVanillaKillButton = false,
    };
}
