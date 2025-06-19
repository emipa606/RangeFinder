using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace RangeFinder;

[StaticConstructorOnStartup]
public class Controller
{
    private static List<ObservedPawn> observedPawns = [];
    private static List<ObservedTargetSearcher> observedTargetSearchers = [];

    private static readonly Material whiteMaterial =
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.white);

    private static readonly Color[] colors =
    [
        Color.red,
        Color.green,
        Color.blue,
        Color.yellow,
        Color.cyan,
        Color.magenta
    ];

    private static readonly Material[] materials =
    [
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.red),
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.green),
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.blue),
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.yellow),
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.cyan),
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, Color.magenta)
    ];

    private static Controller controller;

    private bool isPressed;
    private float lastPressedTime;

    private static Color getColor(int n)
    {
        var customColors = RangeFinder.Settings.customColors;
        return customColors.NullOrEmpty() ? colors[n % colors.Length] : customColors[n % customColors.Count];
    }

    private static Material getMaterial(int n)
    {
        var customColorMaterials = RangeFinder.Settings.customColorMaterials;
        if (customColorMaterials.NullOrEmpty())
        {
            return materials[n % materials.Length];
        }

        var idx = n % customColorMaterials.Count;
        if (customColorMaterials[idx] == null)
        {
            customColorMaterials[idx] = MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent,
                RangeFinder.Settings.customColors[idx]);
        }

        return customColorMaterials[idx];
    }

    public static Controller Instance()
    {
        controller ??= new Controller();
        return controller;
    }

    public static void Reset()
    {
        observedPawns = [];
        observedTargetSearchers = [];
    }

    public static void HandleDrawing()
    {
        if (Find.UIRoot.screenshotMode.Active)
        {
            return;
        }

        var currentMap = Find.CurrentMap;
        var colorIndex = -1;

        var pawns = observedPawns
            .Select(observed => observed.Pawn)
            .Where(pawn => pawn.Map == currentMap && pawn.Spawned && !pawn.Dead && !pawn.Downed);

        var pawnsWithRanges = new HashSet<Pawn>();

        var pawnsArray = pawns as Pawn[] ?? pawns.ToArray();
        pawnsArray
            .Where(pawn =>
            {
                var verb = pawn.equipment?.PrimaryEq?.PrimaryVerb;
                if (verb == null || verb.verbProps.IsMeleeAttack)
                {
                    return false;
                }

                var range = verb.verbProps.range;
                return range > 0 && range < RangeFinder.Settings.maxRange;
            })
            .Do(pawn =>
            {
                var color = RangeFinder.Settings.useColorCoding == BooleanKey.Yes
                    ? getColor(++colorIndex)
                    : Color.white;
                GenDraw.DrawRadiusRing(pawn.Position, pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range, color);
                _ = pawnsWithRanges.Add(pawn);

                if (RangeFinder.Settings.useColorCoding != BooleanKey.Yes)
                {
                    return;
                }

                var mat = getMaterial(colorIndex);
                GenDraw.DrawCircleOutline(pawn.DrawPos, 0.75f, mat);
                GenDraw.DrawCircleOutline(pawn.DrawPos, 0.75f, mat);
            });

        if (RangeFinder.Settings.showRangeAtMouseKey.IsModKeyHeld())
        {
            var selectedPawns = Find.Selector.SelectedPawns
                .Where(pawn =>
                {
                    var verb = pawn.equipment?.PrimaryEq?.PrimaryVerb;
                    if (verb == null || verb.verbProps.IsMeleeAttack)
                    {
                        return false;
                    }

                    var range = verb.verbProps.range;
                    return range > 0 && range < RangeFinder.Settings.maxRange;
                })
                .Except(pawnsWithRanges);

            var selectedPawnsArray = selectedPawns as Pawn[] ?? selectedPawns.ToArray();
            if (selectedPawnsArray.Any())
            {
                var mouseCell = UI.MouseCell();
                GenDraw.DrawCircleOutline(mouseCell.ToVector3Shifted(), 0.25f, whiteMaterial);
                GenDraw.DrawCircleOutline(mouseCell.ToVector3Shifted(), 0.25f, whiteMaterial);

                selectedPawnsArray
                    .Do(pawn =>
                    {
                        var color = RangeFinder.Settings.useColorCoding == BooleanKey.Yes
                            ? getColor(++colorIndex)
                            : Color.white;
                        GenDraw.DrawRadiusRing(mouseCell, pawn.equipment.PrimaryEq.PrimaryVerb.verbProps.range, color);

                        if (RangeFinder.Settings.useColorCoding != BooleanKey.Yes)
                        {
                            return;
                        }

                        var mat = getMaterial(colorIndex);
                        GenDraw.DrawCircleOutline(pawn.DrawPos, 0.75f, mat);
                        GenDraw.DrawCircleOutline(pawn.DrawPos, 0.75f, mat);
                    });
            }
        }

        observedTargetSearchers
            .Select(observed => observed.TargetSearcher)
            .Where(targetSearcher => targetSearcher.Thing != null && targetSearcher.Thing.Map == currentMap &&
                                     targetSearcher.Thing.Spawned && !pawnsArray.Contains(targetSearcher.Thing))
            .Do(targetSearcher =>
            {
                var verb = targetSearcher.CurrentEffectiveVerb;
                if (verb == null)
                {
                    return;
                }

                var range = verb.verbProps.range;
                if (!(range > 0) || !(range < RangeFinder.Settings.maxRange))
                {
                    return;
                }

                var color = RangeFinder.Settings.useColorCoding == BooleanKey.Yes
                    ? getColor(++colorIndex)
                    : Color.white;
                GenDraw.DrawRadiusRing(targetSearcher.Thing.Position, range, color);

                if (RangeFinder.Settings.useColorCoding != BooleanKey.Yes)
                {
                    return;
                }

                var rect = targetSearcher.Thing.OccupiedRect();
                var size = (Mathf.Max(rect.Width, rect.Height) / 2f) + 0.25f;

                var mat = getMaterial(colorIndex);
                GenDraw.DrawCircleOutline(targetSearcher.Thing.DrawPos, size, mat);
                GenDraw.DrawCircleOutline(targetSearcher.Thing.DrawPos, size, mat);
            });
    }

    public void HandleEvents()
    {
        if (RangeFinder.Settings.showWeaponRangeKey.IsModKeyDown())
        {
            if (isPressed)
            {
                return;
            }

            isPressed = true;

            var now = Time.realtimeSinceStartup;
            var locked = now - lastPressedTime <= 0.25f;
            lastPressedTime = now;

            foreach (var pawn in Tools.GetSelectedPawns())
            {
                var observed = observedPawns.FirstOrDefault(c => c.Pawn == pawn);
                if (observed == null)
                {
                    observedPawns.Add(new ObservedPawn(pawn, locked));
                }
                else
                {
                    observedPawns.DoIf(c => c.Pawn == pawn, c => c.Locked = locked);
                }
            }

            foreach (var targetSearcher in Tools.GetSelectedTargetSearchers())
            {
                var observed = observedTargetSearchers.FirstOrDefault(c => c.TargetSearcher == targetSearcher);
                if (observed == null)
                {
                    observedTargetSearchers.Add(new ObservedTargetSearcher(targetSearcher, locked));
                }
                else
                {
                    observedTargetSearchers.DoIf(c => c.TargetSearcher == targetSearcher, c => c.Locked = locked);
                }
            }
        }

        if (!RangeFinder.Settings.showWeaponRangeKey.IsModKeyUp())
        {
            return;
        }

        if (!isPressed)
        {
            return;
        }

        isPressed = false;

        foreach (var pawn in Tools.GetSelectedPawns())
        {
            var observed = observedPawns.FirstOrDefault(c => c.Pawn == pawn);
            if (observed is { Locked: false })
            {
                _ = observedPawns.RemoveAll(c => c.Pawn == pawn);
            }
        }

        foreach (var targetSearcher in Tools.GetSelectedTargetSearchers())
        {
            var observed = observedTargetSearchers.FirstOrDefault(c => c.TargetSearcher == targetSearcher);
            if (observed is { Locked: false })
            {
                _ = observedTargetSearchers.RemoveAll(c => c.TargetSearcher == targetSearcher);
            }
        }
    }
}