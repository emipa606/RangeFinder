using HarmonyLib;
using RimWorld;

namespace RangeFinder.HarmonyPatches;

[HarmonyPatch(typeof(SelectionDrawer), nameof(SelectionDrawer.DrawSelectionOverlays))]
internal static class SelectionDrawer_DrawSelectionOverlays
{
    public static void Prefix()
    {
        Controller.HandleDrawing();
    }
}