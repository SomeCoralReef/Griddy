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
    Attack,
    Spells,
    Items,
    Defend
}