using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Scriptable Objects/ItemData")]
public class ItemData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    public ItemType itemType;
    public int powerAmount;

    public bool isConsumable = true;
    public bool targetsAllies = true;

    public GameObject useVFX;
}


public enum ItemType
{
    Heal,
    Damage,
    Buff,
    Debuff,
    Revive,
    Utility
 }
