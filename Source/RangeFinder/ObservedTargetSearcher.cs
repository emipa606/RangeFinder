using Verse.AI;

namespace RangeFinder;

public class ObservedTargetSearcher(IAttackTargetSearcher forTargetSearcher, bool locked)
{
    public readonly IAttackTargetSearcher TargetSearcher = forTargetSearcher;
    public bool Locked = locked;
}