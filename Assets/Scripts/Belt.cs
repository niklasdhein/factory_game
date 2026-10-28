using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Belt : MonoBehaviour
{
    public ItemHandler itemOnBelt;
    public ItemHandler ItemMovingToThisBelt;
    Transform itemTrans;
    public int beltSpeed;
    public int xDir = 1;
    public int yDir;
    public Animator anim;
    LayerMask objLayer;

    public Belt nextBelt;
    public Belt previousBelt;

    IDHandler iDHandler;
    InventoryHandler inventoryHandler;

    private void Start()
    {
        objLayer = LayerMask.GetMask("Objects");
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();

        //Connect this belt the right way with the surrounding belts
        ConnectBelt();
        UpdateSprite();

        if (itemOnBelt) itemTrans = itemOnBelt.transform;
        else this.enabled = false;
    }

    private void Update()
    {
        if (itemOnBelt && nextBelt && !nextBelt.itemOnBelt && (!nextBelt.ItemMovingToThisBelt || nextBelt.ItemMovingToThisBelt == itemOnBelt)) MoveItemToNextBelt();   
    }

    //Move item along the belt
    public void MoveItemToNextBelt()
    {
        if (!nextBelt.ItemMovingToThisBelt) nextBelt.ItemMovingToThisBelt = itemOnBelt;
        itemTrans.position += beltSpeed * Time.deltaTime * Vector3.Normalize((Vector2)nextBelt.transform.position - (Vector2)itemTrans.position);
        if (Vector2.Distance(nextBelt.transform.position, (Vector2)itemTrans.position) < 0.05f) PassItemToNextBelt();
    }

    public void PassItemToNextBelt()
    {
        if (nextBelt.itemOnBelt) return;
        nextBelt.SetItemOnBelt(itemOnBelt);
        nextBelt.ItemMovingToThisBelt = null;
        itemOnBelt = null;
        this.enabled = false;
    }

    Belt BeltInFront()
    {
        //Get all objects on the position and filter for the belt
        Collider2D col = Physics2D.OverlapBox((Vector2)transform.position + new Vector2(xDir, yDir), new Vector2(.5f, .5f), 0f, objLayer);
        if (col) return col.gameObject.GetComponent<Belt>();
        return null;
    }

    //Set up belt with values of the current transporting item
    public void SetItemOnBelt(ItemHandler item)
    {
        if (itemOnBelt) return;
        itemOnBelt = item;
        itemTrans = item.transform;
        this.enabled = true;
    }

    public void ClickOnBelt()
    {
        //Take item from belt
        if (itemOnBelt && InventoryHandler.correctSlot(itemOnBelt.iD) != -1)
        {
            PickUpItemFromBelt();
        }
        //Place item on belt
        else if (!itemOnBelt && !ItemMovingToThisBelt && InventoryHandler.CountOfSelectedItem() > 0)
        {
            SpawnItemOnBelt(iDHandler.objects[InventoryHandler.iDOfSelectedItem()]);
            inventoryHandler.RemoveItem(1, InventoryHandler.selectedSlot);
        }
    }

    public void SpawnItemOnBelt(GameObject item)
    {
        GameObject _item = Instantiate(item, transform.position, Quaternion.identity);
        _item.GetComponent<ItemHandler>().allowPickup = false;
        SetItemOnBelt(_item.GetComponent<ItemHandler>());
    }

    public void PickUpItemFromBelt()
    {
        //Add item to player inventory
        inventoryHandler.AddItem(itemOnBelt.iD, 1, InventoryHandler.correctSlot(itemOnBelt.iD));

        //Remove item from belt
        Destroy(itemOnBelt.gameObject);
        itemOnBelt = null;
        if (nextBelt && nextBelt.ItemMovingToThisBelt) nextBelt.ItemMovingToThisBelt = null;
        this.enabled = false;
    }

    //Get all belts surrounding this belt
    //If they are not connected connect them with this belt
    //Also set one of these belts as the previous belt of this belt to keep one line up
    public void ConnectBelt()
    {
        List<Belt> belts = GetSurroundingBelts();
        foreach (Belt belt in belts)
        {
            //Try to connect to all belts surrounding this belt
            if (!belt.nextBelt && Vector2.Distance(belt.transform.position, transform.position) == 1)
            {
                //Let the belt point to this belt
                belt.xDir = (int)transform.position.x - (int)belt.transform.position.x;
                belt.yDir = (int)transform.position.y - (int)belt.transform.position.y;
                belt.nextBelt = this;
                belt.UpdateSprite();

                //Set the first belt of the list as the previous belt
                if (!previousBelt)
                {
                    previousBelt = belt;
                    xDir = previousBelt.xDir;
                    yDir = previousBelt.yDir;
                }
            }
        }
        //Check if there is a belt in the front of this belt
        //If so then connect to it
        Belt _belt = BeltInFront();
        if (_belt) nextBelt = _belt;
    }

    public List<Belt> GetSurroundingBelts()
    {
        Collider2D[] colliders = Physics2D.OverlapBoxAll(transform.position, new Vector2(3, 3), 0, objLayer);
        List<Belt> belts = new List<Belt>();
        foreach (Collider2D col in colliders)
        {
            if (col.gameObject.GetComponent<Belt>()) belts.Add(col.gameObject.GetComponent<Belt>());
        }
        return belts;
    }

    public void UpdateSprite()
    {
        //Set belt sprite according to its direction
        if (xDir == 1) anim.SetTrigger("right");
        else if (yDir == -1) anim.SetTrigger("down");
        else if (xDir == -1) anim.SetTrigger("left");
        else if (yDir == 1) anim.SetTrigger("up");
    }

    private void OnDestroy()
    {
        //Allow item to be picked up if the belt gets destroyed
        if (itemOnBelt) itemOnBelt.allowPickup = true;
        if (previousBelt) previousBelt.nextBelt = null;
    }
}
