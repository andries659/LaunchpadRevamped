using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using HarmonyLib;
using LaunchpadRevamped.Features;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Utilities;
using UnityEngine;

namespace LaunchpadRevamped.Patches.Generic;

/// <summary>
/// Splits the freeplay file browser's <c>Modifiers\LPO\</c> folder into Crewmate, Impostor, Neutral, Coven and Universal subfolders.
///
/// MiraAPI builds that folder and fills it with every modifier of the plugin, and its <c>ShowFolder</c> patch is a full
/// replacement, so this patch works around it instead of copying it:
///   1. Before MiraAPI runs, when the Modifiers folder is opened, the faction folders are added to the LPO folder.
///   2. Before MiraAPI runs on a faction folder, that folder briefly pretends to be the LPO folder so MiraAPI lists the
///      plugin's modifiers in it.
///   3. After MiraAPI runs, the folder is restored, the modifiers that don't belong in it are removed and the rest are laid
///      out again.
/// A modifier's faction is the last part of its namespace (<c>LaunchpadRevamped.Modifiers.Game.Coven</c> is Coven).
/// Modifiers in any other namespace stay directly in <c>LPO\</c>.
/// </summary>
[HarmonyPatch(typeof(TaskAdderGame), nameof(TaskAdderGame.ShowFolder))]
public static class FreeplayModifierFolderPatches
{
    private const string ModifiersFolderName = "Modifiers";
    private const string CategoryPrefix = "LPModCat:";
    private const string CloneSuffix = "(Clone)";

    // Going back (GoUpOne) hands ShowFolder the clone we came from, which was destroyed when it was left. Keep the live
    // original of the LPO folder so it can be swapped in.
    private static TaskFolder? _pluginFolder;
    private static string _pluginFolderName = string.Empty;

    private static readonly string[] Categories = { "Crewmate", "Impostor", "Neutral", "Coven", "Universal" };

    public sealed class FolderState
    {
        public TaskFolder? Swapped { get; init; }
        public string OriginalName { get; init; } = string.Empty;
        public string OriginalFolderName { get; init; } = string.Empty;

        /// <summary>The faction whose modifiers belong in the folder being shown, or empty for the LPO folder itself.</summary>
        public string Category { get; init; } = string.Empty;
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    public static void Prefix(TaskAdderGame __instance, ref TaskFolder taskFolder, out FolderState? __state)
    {
        __state = null;

        if (!taskFolder && _pluginFolder && taskFolder.FolderName == _pluginFolderName)
        {
            taskFolder = _pluginFolder;
        }

        if (taskFolder.FolderName == ModifiersFolderName)
        {
            foreach (var sub in taskFolder.SubFolders.ToArray())
            {
                if (Clean(sub.name) == LaunchpadRevampedPlugin.Id)
                {
                    AddCategoryFolders(__instance, sub);
                }
            }

            return;
        }

        var cleanName = Clean(SafeName(taskFolder));
        if (cleanName == LaunchpadRevampedPlugin.Id)
        {
            __state = new FolderState();
            return;
        }

        if (!cleanName.StartsWith(CategoryPrefix))
        {
            return;
        }

        var plugin = MiraPluginManager.GetPluginByGuid(LaunchpadRevampedPlugin.Id);
        if (plugin == null)
        {
            return;
        }

        __state = new FolderState
        {
            Swapped = taskFolder,
            OriginalName = taskFolder.name,
            OriginalFolderName = taskFolder.FolderName,
            Category = cleanName.Substring(CategoryPrefix.Length),
        };

        taskFolder.name = LaunchpadRevampedPlugin.Id;
        taskFolder.FolderName = plugin.PluginInfo.Metadata.Name;
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    public static void Postfix(TaskAdderGame __instance, FolderState? __state)
    {
        if (__state == null)
        {
            return;
        }

        if (__state.Swapped != null)
        {
            __state.Swapped.name = __state.OriginalName;
            __state.Swapped.FolderName = __state.OriginalFolderName;
            RebuildPath(__instance);
        }

        var plugin = MiraPluginManager.GetPluginByGuid(LaunchpadRevampedPlugin.Id);
        if (plugin == null)
        {
            return;
        }

        var modifiersById = new Dictionary<string, BaseModifier>();
        foreach (var modifier in plugin.Modifiers)
        {
            if (modifier.ShowInFreeplay)
            {
                modifiersById[modifier.TypeId.ToString(NumberFormatInfo.InvariantInfo)] = modifier;
            }
        }

        // MiraAPI listed every modifier of the plugin: drop the ones that don't belong in this folder.
        var kept = new List<Transform>();
        foreach (var item in __instance.ActiveItems.ToArray())
        {
            if (modifiersById.TryGetValue(item.name, out var modifier) && GetCategory(modifier) != __state.Category)
            {
                item.gameObject.DeepDestroy(false);
                continue;
            }

            kept.Add(item);
        }

        __instance.ActiveItems.Clear();
        foreach (var item in kept)
        {
            __instance.ActiveItems.Add(item);
        }

        Relayout(__instance);

        var scroller = __instance.TaskParent.GetComponent<Scroller>();
        if (scroller != null)
        {
            scroller.CalculateAndSetYBounds(__instance.ActiveItems.Count, 6, 4.5f, 1.65f);
            scroller.SetYBoundsMin(0.0f);
            scroller.ScrollToTop();
        }
    }

    private static void AddCategoryFolders(TaskAdderGame game, TaskFolder pluginFolder)
    {
        _pluginFolder = pluginFolder;
        _pluginFolderName = pluginFolder.FolderName;

        if (pluginFolder.SubFolders.ToArray().Any(x => Clean(x.name).StartsWith(CategoryPrefix)))
        {
            return;
        }

        var plugin = MiraPluginManager.GetPluginByGuid(LaunchpadRevampedPlugin.Id);
        if (plugin == null)
        {
            return;
        }

        foreach (var category in Categories)
        {
            if (!plugin.Modifiers.Any(m => m.ShowInFreeplay && GetCategory(m) == category))
            {
                continue;
            }

            var folder = Object.Instantiate(game.RootFolderPrefab, game.transform);
            folder.gameObject.SetActive(false);
            folder.FolderName = category;
            folder.name = CategoryPrefix + category;

            var color = GetCategoryColor(category);
            folder.currentFolderColor = color;
            folder.folderSpriteRenderer.color = color;
            folder.buttonRolloverHandler.OutColor = color;
            folder.buttonRolloverHandler.UnselectedColor = color;

            pluginFolder.SubFolders.Add(folder);
        }
    }

    // Same grid as MiraAPI's: folders step by folderWidth, files by fileWidth, wrapping at lineWidth.
    private static void Relayout(TaskAdderGame game)
    {
        var x = 0f;
        var y = 0f;
        var rowHeight = 0f;

        foreach (var item in game.ActiveItems.ToArray())
        {
            var folder = item.GetComponent<TaskFolder>();
            if (folder != null)
            {
                item.localPosition = new Vector3(x, y, 0f);
                rowHeight = Mathf.Max(rowHeight, folder.Text.bounds.size.y + 1.1f);
                x += game.folderWidth;
            }
            else
            {
                var file = item.GetComponent<TaskAddButton>();
                if (file == null)
                {
                    continue;
                }

                item.localPosition = new Vector3(x, y, 0f);
                rowHeight = Mathf.Max(rowHeight, file.Text.bounds.size.y + 1.3f);
                x += game.fileWidth;
            }

            if (x > game.lineWidth)
            {
                x = 0f;
                y -= rowHeight;
                rowHeight = 0f;
            }
        }
    }

    private static void RebuildPath(TaskAdderGame game)
    {
        var builder = new StringBuilder(64);
        foreach (var folder in game.Hierarchy)
        {
            builder.Append(folder.FolderName);
            builder.Append('\\');
        }

        game.PathText.text = builder.ToString();
    }

    private static string GetCategory(BaseModifier modifier)
    {
        var last = modifier.GetType().Namespace?.Split('.').LastOrDefault() ?? string.Empty;
        return Categories.Contains(last) ? last : string.Empty;
    }

    private static Color GetCategoryColor(string category)
    {
        return category switch
        {
            "Crewmate" => Palette.CrewmateBlue,
            "Impostor" => Palette.ImpostorRed,
            "Coven" => LaunchpadPalette.CovenColor,
            "Universal" => new Color(0.196f, 0.659f, 0.322f),
            _ => Color.gray,
        };
    }

    private static string SafeName(TaskFolder folder)
    {
        try
        {
            return folder.name;
        }
        catch
        {
            return string.Empty;
        }
    }

    private static string Clean(string name)
    {
        return name.Replace(CloneSuffix, string.Empty);
    }
}
