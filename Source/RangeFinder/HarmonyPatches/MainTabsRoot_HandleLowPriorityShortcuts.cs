using HarmonyLib;
using RimWorld;

namespace RangeFinder.HarmonyPatches;

[HarmonyPatch(typeof(MainTabsRoot), nameof(MainTabsRoot.HandleLowPriorityShortcuts))]
internal static class MainTabsRoot_HandleLowPriorityShortcuts
{
    public static void Postfix()
    {
        Controller.Instance().HandleEvents();
    }
}