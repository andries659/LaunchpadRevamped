using System;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace LaunchpadRevamped.Features;

public static class LaunchpadAudio
{
    // THIS FILE SHOULD ONLY HOLD AUDIO
    // Sounds are .wav files in Resources/Sounds (MiraAPI can only load .wav from the DLL).

    private const string SoundsPath = "LaunchpadRevamped.Resources.Sounds.";

    // Esper
    public static LoadableAsset<AudioClip> EsperProjectSound { get; } =
        new LoadableAudioResourceAsset(SoundsPath + "EsperProject.wav");

    public static LoadableAsset<AudioClip> EsperReturnSound { get; } =
        new LoadableAudioResourceAsset(SoundsPath + "EsperReturn.wav");

    public static void PlaySound(LoadableAsset<AudioClip> clip, float vol = 1f)
    {
        if (!Constants.ShouldPlaySfx())
        {
            return;
        }

        AudioClip? audio = null;

        try
        {
            audio = clip.LoadAsset();
        }
        catch (Exception e)
        {
            Warning($"[Audio] Could not load a sound: {e.Message}");
        }

        if (!audio)
        {
            return;
        }

        SoundManager.Instance.PlaySound(audio, false, vol);
    }
}
