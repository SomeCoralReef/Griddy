using UnityEngine;

public enum ElementType
{
    Fire,
    Water,
    Thunder,
    Earth,
    Light,
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
    None,
    Attack,
    Spells,
    Items,
    Defend
}