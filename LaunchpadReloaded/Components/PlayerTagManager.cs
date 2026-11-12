using LaunchpadRevamped.Features;
using LaunchpadRevamped.Options;
using MiraAPI.GameOptions;
using MiraAPI.Utilities;
using MonoMod.Utils;
using Reactor.Utilities.Attributes;
using Reactor.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LaunchpadRevamped.Components;

[RegisterInIl2Cpp]
public class PlayerTagManager(IntPtr ptr) : MonoBehaviour(ptr)
{
    public PlayerControl player = null!;
    public PlayerVoteArea? voteArea;

    public Transform tagHolder = null!;
    public GameObject tagTemplate = null!;
    public GridLayoutGroup gridLayout = null!;

    public Transform meetingTagHolder = null!;

    public readonly Dictionary<PlayerTag, GameObject> Tags = [];
    private readonly Dictionary<PlayerTag, GameObject> _oldTags = [];

    private bool _inMeeting;

    public void Awake()
    {
        player = GetComponent<PlayerControl>();

        tagHolder = Instantiate(LaunchpadAssets.PlayerTags.LoadAsset(), player.transform).transform;
        gridLayout = tagHolder.GetComponent<GridLayoutGroup>();

        tagTemplate = tagHolder.transform.GetChild(0).gameObject;
        tagTemplate.SetActive(false);
        tagTemplate.transform.SetParent(player.transform); // Just to store it as a template

        UpdatePosition();
    }

    public void MeetingStart()
    {
        var meeting = MeetingHud.Instance;
        if (meeting != null)
        {
            _inMeeting = true;

            voteArea = meeting.playerStates.FirstOrDefault(plr => plr.PlayerId == player.PlayerId);

            if (voteArea != null)
            {
                meetingTagHolder = Instantiate(LaunchpadAssets.PlayerTags.LoadAsset(), voteArea.transform).transform;
                meetingTagHolder.transform.GetChild(0).gameObject.DestroyImmediate();
                meetingTagHolder.gameObject.layer = voteArea.gameObject.layer;

                Dictionary<PlayerTag, GameObject> toAdd = new();

                foreach (var tagPair in Tags)
                {
                    _oldTags.Add(tagPair.Key, tagPair.Value);

                    var cloneTag = Instantiate(tagPair.Value, meetingTagHolder, false);
                    cloneTag.layer = meetingTagHolder.gameObject.layer;
                    cloneTag.transform.GetChild(0).gameObject.layer = cloneTag.layer;

                    toAdd.Add(tagPair.Key, cloneTag);
                }

                Tags.Clear();
                Tags.AddRange(toAdd);

                meetingTagHolder.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);

                UpdatePosition();
            }
        }
    }

    public void MeetingEnd()
    {
        _inMeeting = false;

        Tags.Clear();

        foreach (var tagPair in _oldTags)
        {
            Tags[tagPair.Key] = tagPair.Value;
        }

        _oldTags.Clear();

        UpdatePosition();
    }

    private const string HostTagName = "Host";
    private static readonly Color HostTagColor = new(0.95f, 0.72f, 0.15f);
    // Lobby tags only exist in the lobby, never during a game.
    private static readonly Func<PlayerControl, bool> HostTagVisible = _ => !!LobbyBehaviour.Instance;
    private float _nextHostTagCheck;

    /// <summary>
    /// Everyone sees a "Host" tag above the lobby host (while the Player Tags option is on).
    /// </summary>
    private void RefreshHostTag()
    {
        if (_inMeeting || !player || !AmongUsClient.Instance)
        {
            return;
        }

        bool tagsEnabled;
        try
        {
            tagsEnabled = OptionGroupSingleton<GeneralOptions>.Instance.PlayerTags;
        }
        catch
        {
            return;
        }

        var wanted = tagsEnabled && !!LobbyBehaviour.Instance && player.OwnerId == AmongUsClient.Instance.HostId;
        var existing = GetTagByName(HostTagName);

        if (wanted && !existing.HasValue)
        {
            AddTag(new PlayerTag(HostTagName, "Host", HostTagColor) { IsLocallyVisible = HostTagVisible, TextOnly = true });
        }
        else if (!wanted && existing.HasValue)
        {
            RemoveTag(existing.Value);
        }
    }

    private const string CustomTagName = "LPR_Tag";

    /// <summary>
    /// Shows the tag from LPR_DATA/Tags that the host gave this player.
    /// </summary>
    private string? _customTagKey;

    private void RefreshCustomTag()
    {
        if (_inMeeting || !player)
        {
            return;
        }

        var entry = default(LobbyTagEntry);
        var has = !!LobbyBehaviour.Instance && LobbyTags.TryGet(player.PlayerId, out entry);
        var key = has ? entry.Text + "|" + entry.Hex : null;
        var existing = GetTagByName(CustomTagName);

        if (existing.HasValue && key != _customTagKey)
        {
            RemoveTag(existing.Value);
            existing = null;
        }

        if (has && !existing.HasValue)
        {
            var colors = LprData.ParseColors(entry.Hex) ?? [Color.white];
            AddTag(new PlayerTag(CustomTagName, entry.Text, colors[0])
            {
                IsLocallyVisible = HostTagVisible,
                TextOnly = true,
                Gradient = colors,
            });
        }

        _customTagKey = key;
    }

    public void Update()
    {
        if (Time.unscaledTime >= _nextHostTagCheck)
        {
            _nextHostTagCheck = Time.unscaledTime + 1f;
            RefreshHostTag();
            RefreshCustomTag();
        }

        foreach (var tagPair in Tags)
        {
            if (!tagPair.Value)
            {
                continue;
            }

            var visible = tagPair.Key.IsLocallyVisible(player)
                          && (_inMeeting ? voteArea!.NameText.gameObject.active : player.cosmetics.nameText.gameObject.active);

            if (!tagPair.Value.active && visible)
            {
                UpdatePosition();
            }

            tagPair.Value.SetActive(visible);
        }
    }

    public int GetActiveCount()
    {
        return Tags.Keys.Count(obj => obj.IsLocallyVisible(player));
    }

    public void UpdatePosition()
    {
        var columnCount = Mathf.CeilToInt((float)GetActiveCount() / gridLayout.constraintCount);

        if (columnCount <= 0)
        {
            return;
        }

        var colorblind = player.cosmetics.colorBlindText.gameObject.active;
        var nameTextY = (colorblind ? 0.85f : 0.69f) + columnCount * 0.27f;
        var holderY = 0.53f + columnCount * 0.15f;

        if (_inMeeting)
        {
            var nameTextPos = voteArea!.NameText.transform.localPosition;

            holderY = -0.07f + columnCount * -0.05f;
            nameTextY = 0.025f + columnCount * 0.1f;

            voteArea!.ColorBlindName.transform.localPosition = new Vector3(-0.9058f, -0.1666f, -0.01f);
            voteArea!.NameText.transform.localPosition = new Vector3(nameTextPos.x, nameTextY, nameTextPos.z);
            meetingTagHolder.transform.localPosition = new Vector3(nameTextPos.x, holderY, 0);
        }
        else
        {
            tagHolder.transform.localPosition = new Vector3(0, holderY, -0.35f);
            player.cosmetics.nameTextContainer.transform.localPosition = new Vector3(0, nameTextY, -0.5f);
        }
    }

    [HideFromIl2Cpp]
    public PlayerTag? GetTagByName(string tagName)
    {
        return Tags.Keys.FirstOrDefault(playerTag => playerTag.Name == tagName);
    }

    [HideFromIl2Cpp]
    public void RemoveTag(PlayerTag tagStruct)
    {
        if (!Tags.TryGetValue(tagStruct, out var playerTag))
        {
            return;
        }

        playerTag.gameObject.DestroyImmediate();
        Tags.Remove(tagStruct);

        UpdatePosition();
    }

    public void ClearTags()
    {
        foreach (var plrTag in Tags)
        {
            RemoveTag(plrTag.Key);
        }
    }

    private static string ApplyGradient(string text, Color[] colors)
    {
        var visible = text.Count(c => !char.IsWhiteSpace(c));
        var result = new System.Text.StringBuilder();
        var index = 0;

        foreach (var c in text)
        {
            if (char.IsWhiteSpace(c))
            {
                result.Append(c);
                continue;
            }

            var t = visible <= 1 ? 0f : index / (float)(visible - 1);
            index++;

            var scaled = t * (colors.Length - 1);
            var from = Mathf.Min(Mathf.FloorToInt(scaled), colors.Length - 1);
            var to = Mathf.Min(from + 1, colors.Length - 1);
            var color = Color.Lerp(colors[from], colors[to], scaled - from);

            result.Append("<color=#").Append(ColorUtility.ToHtmlStringRGB(color)).Append('>').Append(c).Append("</color>");
        }

        return result.ToString();
    }

    [HideFromIl2Cpp]
    public void AddTag(PlayerTag plrTag)
    {
        if (Tags.ContainsKey(plrTag))
        {
            return;
        }

        var newTag = Instantiate(tagTemplate, tagHolder.transform);
        var bgRend = newTag.GetComponent<SpriteRenderer>();
        var tagText = newTag.transform.GetChild(0).GetComponent<TextMeshPro>();
        tagText.text = plrTag.Text;
        newTag.name = plrTag.Name;

        if (plrTag.TextOnly)
        {
            // Just bold, slightly larger text, no background. Optionally with a colour gradient.
            bgRend.enabled = false;

            if (tagText.enableAutoSizing)
            {
                tagText.enableAutoSizing = false;
            }

            tagText.fontStyle |= FontStyles.Bold;
            tagText.enableWordWrapping = false;
            tagText.overflowMode = TextOverflowModes.Overflow;
            tagText.outlineColor = new Color32(0, 0, 0, 255);
            tagText.outlineWidth = 0.15f;

            var body = plrTag.Text;
            if (plrTag.Gradient is { Length: > 1 })
            {
                body = ApplyGradient(plrTag.Text, plrTag.Gradient);
                tagText.color = Color.white;
            }
            else if (plrTag.Color != Color.clear)
            {
                tagText.color = plrTag.Color;
            }

            tagText.text = $"<b><size=130%>{body}</size></b>";
        }
        else if (plrTag.Color != Color.clear)
        {
            tagText.color = plrTag.Color.LightenColor(0.25f);
            bgRend.color = plrTag.Color;
        }

        Tags.Add(plrTag, newTag);

        UpdatePosition();
    }
}

public record struct PlayerTag(string Name, string Text, Color Color)
{
    public Func<PlayerControl, bool> IsLocallyVisible { get; set; } = _ => false;

    /// <summary>
    /// Only show bold, larger text without the background.
    /// </summary>
    public bool TextOnly { get; set; }

    /// <summary>
    /// Colours the text goes through from the first letter to the last. Only used with <see cref="TextOnly"/>.
    /// </summary>
    public Color[]? Gradient { get; set; }
}
