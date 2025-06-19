using Verse;

namespace RangeFinder;

public class ObservedPawn(Pawn forPawn, bool locked)
{
    public readonly Pawn Pawn = forPawn;
    public bool Locked = locked;
}