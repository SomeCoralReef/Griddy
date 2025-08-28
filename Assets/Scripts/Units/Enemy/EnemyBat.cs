using System.Runtime.ExceptionServices;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemyBat : Enemy
{
    [SerializeField] int numberOfLayers = 2;

    [SerializeField] List<GameObject> layerObjects;
    public override void Initialize(EnemyData newData, int spawnPos)
    {
        Debug.Log("Initializing EnemyBat with " + numberOfLayers + " layers.");
        for (int i = 0; i < numberOfLayers; i++)
        {
            Debug.Log($"Initializing layer {i} for EnemyBat");
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
