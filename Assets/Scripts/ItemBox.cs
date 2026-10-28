using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemBox : MonoBehaviour
{
    public int inventorySize;
    public int maxStackSize = 64;
    public GameObject itemDisplay;
    public int[,] objectInventory;

    InventoryHandler inventoryHandler;
    IDHandler iDHandler;

    private void Start()
    {
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();
        //Set objectInventory to its size
        objectInventory = new int[inventorySize, 2];
    }

    public void Interact()
    {
        //Check whether the player pulls items out or puts items in the item box
        if (InventoryHandler.CountOfSelectedItem() == 0) RemoveItemFromSlotAndAddToPlayerInventory(objectInventory[0, 1], 0);
        else AddItemToSlotAndRemoveFromPlayerInventory(InventoryHandler.iDOfSelectedItem(), InventoryHandler.CountOfSelectedItem(), 0);
        UpdateItemDisplayToSlot(0);
    }

    void AddItemToSlotAndRemoveFromPlayerInventory(int id, int amount, int targetSlot)
    {
        if ((id == objectInventory[targetSlot, 0] || objectInventory[targetSlot, 1] <= 0) && InventoryHandler.CountOfSelectedItem() > 0)
        {
            //Remove item from player inventory
            inventoryHandler.SetItem(id, InventoryHandler.CountOfSelectedItem() - PossibleAddableAmountOnSlot(targetSlot, amount), InventoryHandler.selectedSlot);

            //Add item to object inventory
            objectInventory[targetSlot, 0] = id;
            objectInventory[targetSlot, 1] += PossibleAddableAmountOnSlot(targetSlot, amount);

            //Set state that box can't be picked up when it stores items
            gameObject.GetComponent<ObjectHandler>().destructable = false;
        }
    }

    //Add item to slot without checking if its possible
    public void AddItemToSlot(int id, int amount, int targetSlot)
    {
        //Add item to object inventory
        objectInventory[targetSlot, 0] = id;
        objectInventory[targetSlot, 1] += PossibleAddableAmountOnSlot(targetSlot, amount);

        //Set state that box can't be picked up when it stores items
        gameObject.GetComponent<ObjectHandler>().destructable = false;
    }

    void RemoveItemFromSlotAndAddToPlayerInventory(int amount, int targetSlot)
    {
        if (objectInventory[targetSlot, 1] <= 0) return;
        //Add items to player inventory
        int maxAddableAmountToPlayerInventory = InventoryHandler.maxAddableAmountOnSlot(InventoryHandler.correctSlot(objectInventory[targetSlot, 0]), amount);
        inventoryHandler.AddItem(objectInventory[targetSlot, 0], maxAddableAmountToPlayerInventory, InventoryHandler.correctSlot(objectInventory[targetSlot, 0]));

        //Remove as much items as possible
        objectInventory[targetSlot, 1] -= maxAddableAmountToPlayerInventory;

        //Set state that box can be picked up if there are no more items left
        if (objectInventory[targetSlot, 1] <= 0) gameObject.GetComponent<ObjectHandler>().destructable = true;
    }
    
    public void RemoveItemFromSlot(int amount, int targetSlot)
    {
        //Check if its possible to remove the target amount
        if (objectInventory[targetSlot, 1] <= 0 || objectInventory[targetSlot, 1] - amount < 0) return;

        //Remove the items
        objectInventory[targetSlot, 1] -= amount;
        UpdateItemDisplayToSlot(targetSlot);

        //Set state that box can be picked up if there are no more items left
        if (objectInventory[targetSlot, 1] <= 0) gameObject.GetComponent<ObjectHandler>().destructable = true;
    }

    public int PossibleAddableAmountOnSlot(int slot, int targetAmountToAdd)
    {
        if (objectInventory[slot, 1] + targetAmountToAdd > maxStackSize)
        {
            return maxStackSize - objectInventory[slot, 1];
        }
        else
        {
            return targetAmountToAdd;
        }
    }

    public void UpdateItemDisplayToSlot(int targetSlot)
    {
        //Update Item Display
        if (objectInventory[targetSlot, 1] > 0)
        {
            itemDisplay.GetComponent<Image>().sprite = iDHandler.sprites[objectInventory[targetSlot, 0]];
            itemDisplay.GetComponent<Image>().enabled = true;
            itemDisplay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = objectInventory[targetSlot, 1].ToString();
        }
        else
        {
            itemDisplay.GetComponent<Image>().enabled = false;
            itemDisplay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
}
