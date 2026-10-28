using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Connector : MonoBehaviour
{
    ObjectHandler itemInput;
    ObjectHandler itemOutput;

    //Greater than 0 means item is stored, less than 0 means no item stored
    int holdingItem = -1;
    public int itemFilter = -1; //ID of the object the filter looks for

    LayerMask layerMask;
    IDHandler iDHandler;
    InventoryHandler inventoryHandler;

    public Image DisplayBackground;
    public Image Filter;

    public Sprite[] rotationStages = new Sprite[4];
    public Sprite[] shadows = new Sprite[2];
    int rotationIndex = 0;

    private void Start()
    {
        layerMask = LayerMask.GetMask("Objects");
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        SetFilter(-1);
        SetTargets();
    }

    private void FixedUpdate()
    {
        if (!itemInput || !itemOutput) return;
        switch (itemInput.iD)
        {
            //Item box as input
            case 10:
                ItemBoxInput();
                break;
            //Furnace as input
            case 14:
                FurnaceInput();
                break;
            //Belt as input
            case 18:
                BeltInput();
                break;
            //Object Miner as input
            case 24:
                ObjectMinerInput();
                break;
        }
        switch (itemOutput.iD)
        {
            //Item box as output
            case 10:
                ItemBoxOutput();
                break;
            //Furnace as output
            case 14:
                FurnaceOutput();
                break;
            //Belt as output
            case 18:
                BeltOutput();
                break;
        }
        UpdateUI();
    }

    public void Interact()
    {
        //Check if the player wants to set a filter 
        if (InventoryHandler.CountOfSelectedItem() > 0)
        {
            SetFilter(InventoryHandler.iDOfSelectedItem());
            Debug.Log("Filter set to " + IDHandler.IDToName(itemFilter));
        }

        //Check if player has selected a free slot
        //-> intention to pick up the item the connector is holding
        else if (InventoryHandler.CountOfSelectedItem() <= 0 && holdingItem != -1)
        {
            int slot = InventoryHandler.correctSlot(holdingItem);
            if (slot != -1) inventoryHandler.AddItem(holdingItem, 1, slot);
            holdingItem = -1;
        }
        //If not the player's intention is to rotate the connector
        else Rotate();
    }

    public void Rotate()
    {
        //Rotate, then get the new input and output
        if (rotationIndex + 1 > 3) rotationIndex = 0;
        else rotationIndex++;
        //Change Sprite and shadow according to rotation
        transform.GetChild(0).GetComponent<SpriteRenderer>().sprite = rotationStages[rotationIndex];
        if (transform.GetChild(2).GetComponent<SpriteRenderer>().sprite == shadows[0]) transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = shadows[1];
        else transform.GetChild(2).GetComponent<SpriteRenderer>().sprite = shadows[0];
        SetTargets();
    }

    void SetTargets()
    {
        //Get input and output of the connector
        Collider2D input_col, output_col;
        switch (rotationIndex)
        {
            case 0:
                input_col = Physics2D.OverlapBox(transform.position + Vector3.left, new Vector2(.5f, .5f), 0, layerMask);
                output_col = Physics2D.OverlapBox(transform.position + Vector3.right, new Vector2(.5f, .5f), 0, layerMask);
                break;
            case 1:
                input_col = Physics2D.OverlapBox(transform.position + Vector3.down, new Vector2(.5f, .5f), 0, layerMask);
                output_col = Physics2D.OverlapBox(transform.position + Vector3.up, new Vector2(.5f, .5f), 0, layerMask);
                break;
            case 2:
                input_col = Physics2D.OverlapBox(transform.position + Vector3.right, new Vector2(.5f, .5f), 0, layerMask);
                output_col = Physics2D.OverlapBox(transform.position + Vector3.left, new Vector2(.5f, .5f), 0, layerMask);
                break;
            default:
                input_col = Physics2D.OverlapBox(transform.position + Vector3.up, new Vector2(.5f, .5f), 0, layerMask);
                output_col = Physics2D.OverlapBox(transform.position + Vector3.down, new Vector2(.5f, .5f), 0, layerMask);
                break;
        }

        //If input and output are valid get their object handler
        if (input_col && output_col)
        {
            itemInput = input_col.gameObject.GetComponent<ObjectHandler>();
            itemOutput = output_col.gameObject.GetComponent<ObjectHandler>();
            Debug.Log(itemInput.name + itemOutput.name);
            return;
        }
        //Unlink input and output when the new targets are not valid
        itemInput = null;
        itemOutput = null;
        Debug.Log("Connector has been unlinked");
    }

    void UpdateUI()
    {
        if (holdingItem >= 0)
        {
            DisplayBackground.gameObject.SetActive(true);
            DisplayBackground.transform.GetChild(0).GetComponent<Image>().sprite = iDHandler.sprites[holdingItem];
        }
        else DisplayBackground.gameObject.SetActive(false);
    }

    bool validInput(int targetID)
    {
        if (targetID == itemFilter || itemFilter == -1) return true;
        return false;
    }

    void SetFilter(int filterID)
    {
        itemFilter = filterID;
        if (itemFilter != -1)
        {
            Filter.sprite = iDHandler.sprites[itemFilter];
            Filter.enabled = true;
        }
        else Filter.enabled = false;
    }


    void ObjectMinerInput()
    {
        //Get item from the object miner and set it as holding item
        ObjectMiner objectMiner = itemInput.GetComponent<ObjectMiner>();
        if (holdingItem >= 0 || objectMiner.holdingItem == -1 || objectMiner.holdingItemAmount <= 0 || !validInput(objectMiner.holdingItem)) return;
        holdingItem = objectMiner.holdingItem;
        objectMiner.RemoveItems(1);
    }

    void FurnaceInput()
    {
        //Get item from furnace and set it as holding item
        Furnace furnace = itemInput.GetComponent<Furnace>();
        if (!furnace.smeltingOre || furnace.animator.GetBool("smelt") || holdingItem >= 0 || !validInput(furnace.smeltingOre.iD)) return;
        holdingItem = furnace.smeltingOre.iD;
        furnace.smeltingOre = null;
        furnace.ItemDisplay.enabled = false;
    }

    void FurnaceOutput()
    {
        //Put holding item in furnace
        Furnace furnace = itemOutput.GetComponent<Furnace>();
        if (holdingItem == -1 || !furnace.CanInteract(iDHandler.objects[holdingItem].GetComponent<ItemHandler>())) return;
        furnace.Interact(iDHandler.objects[holdingItem].GetComponent<ItemHandler>());
        holdingItem = -1;
    }

    void BeltInput()
    {
        //Get item from belt and set it as holding item
        Belt belt = itemInput.GetComponent<Belt>();
        if (!belt.itemOnBelt || holdingItem >= 0 || !validInput(belt.itemOnBelt.iD)) return;
        holdingItem = belt.itemOnBelt.iD;
        //Destroy the item on the belt and disable the belt
        Destroy(belt.itemOnBelt.gameObject);
        belt.itemOnBelt = null;
        belt.enabled = false;
    }

    void BeltOutput()
    {
        //Put holding item on the belt
        Belt belt = itemOutput.GetComponent<Belt>();
        if (belt.itemOnBelt || belt.ItemMovingToThisBelt || holdingItem == -1) return;
        GameObject item = Instantiate(iDHandler.objects[holdingItem], belt.transform.position, Quaternion.identity);
        item.GetComponent<ItemHandler>().allowPickup = false;
        belt.SetItemOnBelt(item.GetComponent<ItemHandler>());
        holdingItem = -1;
    }

    void ItemBoxInput()
    {
        //Pull one item from box and place it as holding item
        ItemBox itemBox = itemInput.GetComponent<ItemBox>();
        if (itemBox.objectInventory[0, 1] <= 0 || holdingItem >= 0 || !validInput(itemBox.objectInventory[0, 0])) return;
        holdingItem = itemBox.objectInventory[0, 0];
        itemBox.RemoveItemFromSlot(1, 0);
        itemBox.UpdateItemDisplayToSlot(0);
    }

    void ItemBoxOutput()
    {
        //Put holding item in an item box
        ItemBox itemBox = itemOutput.GetComponent<ItemBox>();
        if (holdingItem >= 0 && (itemBox.objectInventory[0, 1] <= 0 || itemBox.objectInventory[0, 0] == holdingItem) && itemBox.objectInventory[0, 1] + 1 <= itemBox.maxStackSize)
        {        
            itemBox.AddItemToSlot(holdingItem, 1, 0);
            itemBox.UpdateItemDisplayToSlot(0);
            holdingItem = -1;
        }
    }
}
