using UnityEngine;
using System.Collections;
[CreateAssetMenu(fileName = "SpellData", menuName = "Action Data/Spells")]
public class SpellData : ActionData
{
    public int manaCost;
    public string spellDescription;
    public ElementType elementType;

    public bool targetAllies = true;
    public bool needsTarget;

    public GameObject useVFX;

    public override IEnumerator ExecuteAction(Player player, int targetSlotIndex)
    {

    }
}
