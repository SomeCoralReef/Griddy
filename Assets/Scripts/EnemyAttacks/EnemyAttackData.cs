using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackData", menuName = "Scriptable Objects/EnemyAttackData")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public float power;
    public ElementType elementType;
    public string effect;   
}
