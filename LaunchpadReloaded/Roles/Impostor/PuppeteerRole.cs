using System;
using System.Collections.Generic;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Ported from TOHO (Puppeteer). A puppet kills the nearest non-Impostor it walks up to, then is released.
/// </summary>
public class PuppeteerRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    /// <summary>
    /// Players currently under control. Only tracked on the Puppeteer's own client, which also does the killing.
    /// </summary>
    public static readonly HashSet<byte> Puppets = [];

    public Color RoleColor => LaunchpadPalette.PuppeteerColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.ImpostorConcealing;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.PuppetButton,
        UseVanillaKillButton = false,
    };
}
