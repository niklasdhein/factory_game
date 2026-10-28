using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public GameObject[] ObjectPrefabs;
    public int radius;
    public float spawnRate;     //In seconds
    public bool isActive = true;
    public Vector2 placeOffset;

    LayerMask objectLayer;

    private void Start()
    {
        objectLayer = LayerMask.GetMask("Objects");
        if (isActive) StartCoroutine(SpawnCycle());
    }

    IEnumerator SpawnCycle()
    {
        while (isActive)
        {
            //Choose a point in square with edge length 2 * radius, then calculate direction vector
            Vector2 dir = new Vector2(Random.Range(-radius, radius), Random.Range(-radius, radius)) + (Vector2)transform.position - (Vector2)transform.position;
            //Set magnitude to 1
            dir = dir.normalized;
            //Choose a distance
            dir *= Random.Range(0, radius);
            //Round to a valid grid position
            Vector2 pos = (Vector2)transform.position + dir;
            pos = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y)) + placeOffset;
            //Check if the object intersects
            int index = Mathf.RoundToInt(Random.Range(0, ObjectPrefabs.Length));
            if (CanSpawn(ObjectPrefabs[index].GetComponent<BoxCollider2D>(), pos)) Instantiate(ObjectPrefabs[index], pos, Quaternion.identity);
            yield return new WaitForSeconds(spawnRate);
        }
    }

    bool CanSpawn(BoxCollider2D col, Vector2 tarPos)
    {
        Collider2D[] collisions = Physics2D.OverlapBoxAll(tarPos, new Vector2(.5f * col.size.x, .5f * col.size.y), 0, objectLayer);
        if (collisions.Length == 0) return true;
        return false;
    }
}
