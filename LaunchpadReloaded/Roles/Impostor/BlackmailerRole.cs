using System;
using System.Collections.Generic;
using System.Linq;
using LaunchpadRevamped.Features;
using MiraAPI.Roles;
using UnityEngine;

namespace LaunchpadRevamped.Roles.Impostor;

/// <summary>
/// Ported from TOHO (Blackmailer). Silences one player in the next meeting.
/// </summary>
public class BlackmailerRole(IntPtr ptr) : ImpostorRole(ptr), ICustomRole
{
    /// <summary>
    /// Blackmailer id -> blackmailed player id. Kept on every client (synced by RpcBlackmail) because the
    /// victim's chat is blocked on their own client and the meeting shows the mark to everybody.
    /// </summary>
    public static readonly Dictionary<byte, byte> Blackmailed = [];

    public static bool IsBlackmailed(byte playerId) => Blackmailed.Values.Contains(playerId);

    public Color RoleColor => LaunchpadPalette.BlackmailerColor;
    public RoleOptionsGroup RoleOptionsGroup => LaunchpadRoleGroups.ImpostorSupport;
    public ModdedRoleTeams Team => ModdedRoleTeams.Impostor;

    public CustomRoleConfiguration Configuration => new(this)
    {
        Icon = LaunchpadAssets.BlackmailButton,
    };
}
