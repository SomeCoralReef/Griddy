using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Collections;


public class EnemyGolem : Enemy
{
    private int yDirection = 1;
    [SerializeField] int numberOfLayers = 2;
    [SerializeField] List<GameObject> layerObjects;
    public override void Initialize(EnemyData newData, int spawnPos)
    {
        for (int i = 0; i < numberOfLayers; i++)
        {
            layerObjects.Add(Instantiate(newData.layerPrefabs[i], transform));
            layerObjects[i].SetActive(false);
        }
        health = 2;
        base.Initialize(newData, spawnPos);
    }

    public override void CheckLayers(ElementType type)
    {
        for (int i = 0; i < numberOfLayers; i++)
        {
            if (runtimeWeaknesses[i] == type)
            {
                layerObjects[i].SetActive(true);
                Debug.Log(layerObjects[i].activeSelf);
                StartCoroutine(CheckIfStillActiveNextFrame(layerObjects[i]));
                Debug.Log(layerObjects[i].name + " activated by " + type);
            }
            else
            {
                
            }
        }
    }

    private IEnumerator CheckIfStillActiveNextFrame(GameObject obj)
    {
        yield return null; // wait 1 frame
        Debug.Log($"{obj.name} active status after 1 frame: {obj.activeSelf}");
    }


}
