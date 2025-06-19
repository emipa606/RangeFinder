using HarmonyLib;
using Verse;

namespace RangeFinder.HarmonyPatches;

[HarmonyPatch(typeof(Map), nameof(Map.FinalizeLoading))]
internal static class Map_FinalizeLoading
{
    public static void Postfix()
    {
        Controller.Reset();
    }
}