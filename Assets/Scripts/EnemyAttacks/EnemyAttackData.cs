using UnityEngine;

[CreateAssetMenu(fileName = "EnemyAttackData", menuName = "Scriptable Objects/EnemyAttackData")]
public class EnemyAttackData : ScriptableObject
{
    public string attackName;
    public int power;
    public ElementType elementType;
    public string effect;   

    [Header("VFX")]
    public GameObject vfxPrefab; // generic prefab with SpriteVFX
    public Sprite[] hitVFXFrames;
    public float hitVFXFrameRate = 12f;
}
