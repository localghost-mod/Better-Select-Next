using System.Linq;
using HarmonyLib;
using UnityEngine;
using Verse;

namespace Better_Select_Next
{
    [StaticConstructorOnStartup]
    static class Startup
    {
        static Startup() => new Harmony("localghost.betterselectnext").PatchAll();
    }

    static class Utility
    {
        public static Thing GetNext(Thing thing, int offset)
        {
            var list = (
                Find.ColonistBar.GetColonistsInOrder().Contains(thing as Pawn)
                    ? Find.ColonistBar.GetColonistsInOrder().Select(x => x as Thing)
                    : (
                        thing is Pawn pawn
                            ? thing.Map.mapPawns.AllPawnsSpawned.Where(x => x.RaceProps == pawn.RaceProps && x.Faction == pawn.Faction).Select(x => x as Thing)
                            : thing.Map.listerThings.ThingsOfDef(thing.def)
                    ).OrderBy(x => CellIndicesUtility.CellToIndex(x.Position, thing.Map.Size.x))
            ).Where(x => !x.Position.Fogged(thing.Map));
            return list.ElementAt((list.FirstIndexOf(t => t == thing) + offset + list.Count()) % list.Count());
        }
    }

    [HarmonyPatch("RimWorld.ThingSelectionUtility", "SelectNextColonist")]
    class SelectNextPatch
    {
        static bool Prefix()
        {
            var things = Find.Selector.SelectedObjectsListForReading;
            if (things.Count != 1)
                return true;
            var thing = things[0] as Thing;
            if (thing == null || !thing.Spawned)
                return true;
            var target = Utility.GetNext(thing, 1);
            CameraJumper.TrySelect(target);
            if (!Event.current.shift)
                CameraJumper.TryJump(target);
            return false;
        }
    }

    [HarmonyPatch("RimWorld.ThingSelectionUtility", "SelectPreviousColonist")]
    class SelectPrevPatch
    {
        static bool Prefix()
        {
            var things = Find.Selector.SelectedObjectsListForReading;
            if (things.Count != 1)
                return true;
            var thing = things[0] as Thing;
            if (thing == null || !thing.Spawned)
                return true;
            var target = Utility.GetNext(thing, -1);
            CameraJumper.TrySelect(target);
            if (!Event.current.shift)
                CameraJumper.TryJump(target);
            return false;
        }
    }
}
