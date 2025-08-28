using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : ScriptableObject
{
    public string enemyName;
    public float speed;
    public int damage;
    public int health;
    public int maxHealth;
    public ElementType[] weaknesses = new ElementType[3];
    public List<GameObject> layerPrefabs; // Prefabs for each layer

}
