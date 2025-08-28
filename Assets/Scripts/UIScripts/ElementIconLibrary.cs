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
            ElementType.Red => fireIcon,
            ElementType.Blue => waterIcon,
            ElementType.Green => earthIcon,
            ElementType.Yellow => lightIcon,
            ElementType.Purple => thunderIcon,
            ElementType.Orange => lightIcon,
            _ => null
        };
    }
}
