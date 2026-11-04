using System;
using System.Linq;
using AmongUs.GameOptions;
using BepInEx.Configuration;
using HarmonyLib;
using MiraAPI.Roles;
using Reactor.Utilities;

namespace LaunchpadRevamped.Features.TestClients;

/// <summary>
/// Optional forced roles for test games, set in the BepInEx config (Test Clients section).
/// Role names are the class name with or without "Role" (Esper, EsperRole, Sheriff...) or a vanilla role (Impostor...).
/// Leave empty to keep the normal random assignment.
/// </summary>
public static class TestClientRoles
{
    private static ConfigEntry<string>? _humanRole;
    private static ConfigEntry<string>? _botRole;

    private static void EnsureConfig()
    {
        if (_humanRole != null)
        {
            return;
        }

        var config = PluginSingleton<LaunchpadRevampedPlugin>.Instance.GetConfigFile();
        _humanRole = config.Bind("Test Clients", "Your Role", "",
            "Role forced onto you in test games (e.g. Esper). Empty = random.");
        _botRole = config.Bind("Test Clients", "Bot Role", "",
            "Role forced onto every bot in test games (e.g. Sheriff). Empty = random.");
    }

    private static bool TryResolve(string name, out RoleTypes type)
    {
        type = default;
        name = name.Trim();
        if (name.Length == 0)
        {
            return false;
        }

        var custom = CustomRoleManager.CustomRoleBehaviours.FirstOrDefault(r =>
        {
            var typeName = r.GetType().Name;
            return typeName.Equals(name, StringComparison.OrdinalIgnoreCase) ||
                   typeName.Equals(name + "Role", StringComparison.OrdinalIgnoreCase);
        });

        if (custom != null)
        {
            type = custom.Role;
            return true;
        }

        return Enum.TryParse(name, true, out type);
    }

    /// <summary>Runs on the host after the normal assignment.</summary>
    public static void Apply()
    {
        EnsureConfig();

        var hasHuman = TryResolve(_humanRole!.Value, out var humanRole);
        var hasBot = TryResolve(_botRole!.Value, out var botRole);

        foreach (var player in PlayerControl.AllPlayerControls.ToArray())
        {
            if (!player || player.Data == null)
            {
                continue;
            }

            var isBot = TestClientManager.IsBot(player.PlayerId);
            if (isBot && hasBot)
            {
                player.RpcSetRole(botRole, true);
            }
            else if (!isBot && hasHuman)
            {
                player.RpcSetRole(humanRole, true);
            }
        }
    }
}

[HarmonyPatch(typeof(RoleManager), nameof(RoleManager.SelectRoles))]
public static class TestClientRolePatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void SelectRolesPostfix()
    {
        if (TestClientManager.Enabled && TestClientManager.HasBots && AmongUsClient.Instance.AmHost)
        {
            TestClientRoles.Apply();
        }
    }
}
