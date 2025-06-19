using HarmonyLib;
using UnityEngine;
using Verse;

namespace RangeFinder;

internal class RangeFinder : Mod
{
    public static RangeFinderSettings Settings;

    public RangeFinder(ModContentPack content) : base(content)
    {
        Settings = GetSettings<RangeFinderSettings>();

        var harmony = new Harmony("net.pardeike.rimworld.mods.rangefinder");
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        RangeFinderSettings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Range Finder";
    }
}