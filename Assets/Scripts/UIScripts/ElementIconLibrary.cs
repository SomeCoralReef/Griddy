using UnityEngine;

[CreateAssetMenu(fileName = "ElementIconLibrary", menuName = "Scriptable Objects/ElementIconLibrary")]
public class ElementIconLibrary : ScriptableObject
{
    public Sprite fireIcon;
    public Sprite waterIcon;
    public Sprite earthIcon;
    public Sprite thunderIcon;
    public Sprite lightIcon;



    public Sprite GetIcon(ElementType element)
    {
        return element switch
        {
            ElementType.Fire => fireIcon,
            ElementType.Water => waterIcon,
            ElementType.Earth => earthIcon,
            ElementType.Thunder => thunderIcon,
            ElementType.Light => lightIcon,
            _ => null
        };
    }
}
