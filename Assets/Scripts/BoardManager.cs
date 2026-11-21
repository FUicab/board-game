using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField]
    List<TileRegistry> tilePrefabs;
    [SerializeField]
    Transform tileContainer;
    [SerializeField]
    public LayerMask Blocker;

    List<int> tileIndexes = new(); //Repeats every index number for a tile prefab equal to its tile count

    float tileWidth = 6f;
    int xTileCount = 6;
    int zTileCount = 6;
    Vector3 startPosition = new Vector3(15, 0, 15);

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < tilePrefabs.Count; i++)
        {
            for (int x = 0; x < tilePrefabs[i].tileCount; x++)
            {
                tileIndexes.Add(i);
            }
        }

        for (int x = 0; x < xTileCount; x++)
        {
            for (int z = 0; z < zTileCount; z++)
            {
                int i = tileIndexes[UnityEngine.Random.Range(0, tileIndexes.Count)];
                TileRegistry tile = tilePrefabs[i];
                tileIndexes.RemoveAt(i);
                Vector3 tilePosition = new Vector3(startPosition.x - (tileWidth * x), 0, startPosition.z - (tileWidth * z));


                Quaternion tileRotation = Quaternion.Euler(0, UnityEngine.Random.Range(0, 4) * 90f, 0);
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    bool blocked = false;
                    switch (tileRotation.eulerAngles.y)
                    {
                        case 0f:
                            if (Physics.Raycast(tilePosition, Vector3.forward * 4, 4, Blocker)) blocked = true;
                            //Debug.DrawRay(tilePosition, Vector3.forward * 4, Color.aliceBlue, 10f);
                            break;
                        case 90f:
                            if (Physics.Raycast(tilePosition, Vector3.right * 4, 4, Blocker)) blocked = true;
                            //Debug.DrawRay(tilePosition, Vector3.right * 4, Color.aliceBlue, 10f);
                            break;
                        case 180f:
                            if (Physics.Raycast(tilePosition, Vector3.back * 4, 4, Blocker)) blocked = true;
                            //Debug.DrawRay(tilePosition, Vector3.back * 4, Color.aliceBlue, 10f);
                            break;
                        case 270f:
                            if (Physics.Raycast(tilePosition, Vector3.left * 4, 4, Blocker)) blocked = true;
                            //Debug.DrawRay(tilePosition, Vector3.left * 4, Color.aliceBlue, 10f);
                            break;
                    }
                    if(blocked)
                    {
                        tileRotation = Quaternion.Euler(0, tileRotation.eulerAngles.y + 90f, 0);
                    } else
                    {
                        break;
                    }
                }

                GameObject newTile = Instantiate(tile.tilePrefab, tilePosition, tileRotation, tileContainer);
                newTile.GetComponent<TileManager>().SetRotation(tileRotation.eulerAngles.y);
                //newTile.transform.rotation *= tileRotation;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

[Serializable]
public class TileRegistry
{
    [SerializeField]
    public GameObject tilePrefab;
    [SerializeField]
    public int tileCount = 1;
}