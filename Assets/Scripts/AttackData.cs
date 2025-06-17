using UnityEngine;

[CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/AttackData")]
public class AttackData : ScriptableObject
{
    public string attackName;
    public ElementType elementType;
    public float power;
    public int[] patternOffsets;
}
