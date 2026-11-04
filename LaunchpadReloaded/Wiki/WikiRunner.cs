using System;
using Il2CppInterop.Runtime.Injection; // older BepInEx 6 builds: using UnhollowerRuntimeLib;
using UnityEngine;
using Object = UnityEngine.Object;

namespace LaunchpadReloaded.Wiki; // <-- change to your mod's namespace

/// <summary>Tiny MonoBehaviour that gives the wiki an Update loop (menu and in-game).</summary>
public class WikiRunner : MonoBehaviour
{
    public WikiRunner(IntPtr ptr) : base(ptr) { } // required for Il2Cpp-injected classes

    private void Update()
    {
        WikiPanel.Tick();

        // TEMPORARY test hotkey - delete this once your own open button exists.
        if (Input.GetKeyDown(KeyCode.F1)) WikiPanel.Toggle();
    }
}

public static class WikiBootstrap
{
    private static bool _done;

    /// <summary>Call once from your plugin's Load().</summary>
    public static void Init()
    {
        if (_done) return;
        _done = true;

        ClassInjector.RegisterTypeInIl2Cpp<WikiRunner>();

        var go = new GameObject("LaunchpadWikiRunner");
        Object.DontDestroyOnLoad(go);
        go.hideFlags = HideFlags.HideAndDontSave;
        go.AddComponent<WikiRunner>();
    }
}

/* ---------------------------------------------------------------------------
   USAGE
   -----
   In your BasePlugin.Load():
       WikiBootstrap.Init();

   Register roles (anywhere before the wiki is opened):
       WikiRegistry.Register(new WikiRole
       {
           Name = "Example",
           Team = "Crewmate",
           Color = new Color(0.4f, 0.8f, 1f),
           Description = "What the role does overall.",
           Icon = WikiAssets.FromEmbeddedPng("Wiki.example.png"),
           Abilities =
           {
               new WikiAbility { Name = "Ping", Cooldown = "Cooldown: 25s", Description = "What it does.", Icon = ... }
           }
       });

   Your own open button just calls:
       WikiPanel.Open();   // or Close() / Toggle()
--------------------------------------------------------------------------- */
