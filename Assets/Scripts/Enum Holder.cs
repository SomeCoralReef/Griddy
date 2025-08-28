using UnityEngine;

public enum ElementType
{
    Red,
    Blue,
    Yellow,
    Green,
    Purple,
    Orange,
    None
}


public enum TimelineState
{
    Idle,
    Preparing,
    Paused,
    Executing
}

public enum CommandCategory
{
    Attack,
    Spells,
    Items,
    Defend
}