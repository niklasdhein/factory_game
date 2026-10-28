using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemHandler : MonoBehaviour
{
    public int iD;
    public bool destroyWhenTargetReached = false;
    public bool allowPickup = true;
    public int speed = 5;
    public bool isPlacable;
    public Vector2 placeOffset;
    public GameObject PlacedObject;

    private Transform targetTrans;
    private Vector2 targetVec;

    private void FixedUpdate()
    {
        if (targetTrans && Vector2.Distance(targetTrans.position, transform.position) > 0.1f) MoveToTarget((Vector2)targetTrans.position);
        else if (targetVec != Vector2.zero && Vector2.Distance(targetVec, transform.position) > 0.1f) MoveToTarget(targetVec);
    }

    //Moving functions
    public void SetTargetTransform(Transform trans)
    {
        targetTrans = trans;
    }
    public void SetTargetVector(Vector2 vec)
    {
        targetVec = vec;
    }
    void MoveToTarget(Vector2 target)
    {
        transform.position += ((Vector3)target - transform.position) * speed * Time.fixedDeltaTime;
        if (destroyWhenTargetReached && Vector2.Distance(target, transform.position) < 0.2f) DestroyItem();
    }

    //Place item if its placable
    public void Place()
    {
        if (!isPlacable || !validSpot() || InventoryHandler.CountOfSelectedItem() <= 0 || CursorManager.selectorOnObject) return;
        //New reference because the item is not actually in the scene (only prefab) so start does not get loaded
        GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>().RemoveItem(InventoryHandler.maxSubtractableAmountOnSlot(InventoryHandler.selectedSlot, 1), InventoryHandler.selectedSlot);
        Instantiate(PlacedObject, CursorManager.selectorPos + placeOffset, Quaternion.identity);
    }

    void DestroyItem()
    {
        Destroy(gameObject);
    }

    bool validSpot()
    { 
        BoxCollider2D col = PlacedObject.GetComponent<BoxCollider2D>();
        //Make the overlapping box a little bit smaller to prevent collisions on edges which would be allowed
        if (Physics2D.OverlapBox(CursorManager.selectorPos + placeOffset, new Vector2(col.size.x - .1f, col.size.y - .1f), 0)) return false;
        return true;
    }
}
