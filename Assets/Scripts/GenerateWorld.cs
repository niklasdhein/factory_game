using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenerateWorld : MonoBehaviour
{
    public GameObject TilePrefab;
    [Range (1, 100)]
    public int mapSizeX, mapSizeY;

    // Start is called before the first frame update
    void Start()
    {
        for (int y = 0; y < mapSizeY; y++)
        {
            for (int x = 0; x < mapSizeX; x++)
            {
                Instantiate(TilePrefab, transform.position + new Vector3(x, y), Quaternion.Euler(0, 0, 0)).transform.SetParent(transform);
            }
        }
    }
}
