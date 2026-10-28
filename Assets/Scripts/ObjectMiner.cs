using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectMiner : MonoBehaviour
{
    public bool isActive = true;
    public int miningRate;      //Rate in seconds
    public int range;
    public List<ObjectHandler> targetObjects;
    [HideInInspector]
    public int holdingItem = -1;     //Item ID
    [HideInInspector]
    public int holdingItemAmount = 0;

    LayerMask objectLayer;
    IDHandler iDHandler;
    Image itemDisplay;
    TextMeshProUGUI amountDisplay;
    InventoryHandler inventoryHandler;

    private void Start()
    {
        //Set up references
        objectLayer = LayerMask.GetMask("Objects");
        itemDisplay = transform.GetChild(1).GetChild(0).GetComponent<Image>();
        amountDisplay = itemDisplay.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();

        StartCoroutine(WorkCycle());
    }

    //Event called when player right clicks on miner
    public void Interact()
    {
        if (!isActive || holdingItem == -1 || holdingItemAmount == 0 || InventoryHandler.correctSlot(holdingItem) == -1) return;
        //Add max addable amount to the player inventory
        int maxAddableAmountToPlayerInventory = InventoryHandler.maxAddableAmountOnSlot(InventoryHandler.correctSlot(holdingItem), holdingItemAmount);
        inventoryHandler.AddItem(holdingItem, maxAddableAmountToPlayerInventory, InventoryHandler.correctSlot(holdingItem));
        //Remove items from the miner
        RemoveItems(maxAddableAmountToPlayerInventory);
    }

    IEnumerator WorkCycle()
    {
        while (isActive)
        {
            //Work if the miner is able to work / empty slot
            if (holdingItem == -1)
            {
                //Get all colliders of objects in the miners range
                Collider2D[] colliders = Physics2D.OverlapCircleAll((Vector2)transform.position, range, objectLayer);

                //Mine one object and place the items dropped in miners slot
                ObjectHandler targetObject = TargetObject(colliders);
                if (targetObject)
                {
                    holdingItem = targetObject.objectDrops[0].GetComponent<ItemHandler>().iD;
                    holdingItemAmount = Mathf.RoundToInt(Random.Range(targetObject.rangeMinDropAmount, targetObject.rangeMaxDropAmount));
                    UpdateUI();
                    targetObject.objectDrops = null;
                    Destroy(targetObject.gameObject);
                    //Debug.Log("Now holding " + IDHandler.IDToName(holdingItem));
                }
            }
            yield return new WaitForSeconds(miningRate);
        }
    }

    ObjectHandler TargetObject(Collider2D[] colliders)
    {
        //Get a list of all the objects the miner could mine
        List<ObjectHandler> compatibleObjects = new List<ObjectHandler>();
        for (int i = 0; i < colliders.Length; i++)
        {
            for (int j = 0; j < targetObjects.Count; j++)
            {
                //Check for matching object iDs
                if (targetObjects[j].iD == colliders[i].GetComponent<ObjectHandler>().iD) compatibleObjects.Add(colliders[i].GetComponent<ObjectHandler>());
            }
        }
        if (compatibleObjects.Count == 0) return null;
        //Pick a random object for the miner
        int index = Mathf.RoundToInt(Random.Range(0, compatibleObjects.Count));
        return compatibleObjects[index];
    }

    public void RemoveItems(int amount)
    {
        if (holdingItem == -1 || holdingItemAmount <= 0 || holdingItemAmount - amount < 0) return;
        holdingItemAmount -= amount;
        if (holdingItemAmount <= 0) holdingItem = -1;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (holdingItem == -1)
        {
            itemDisplay.enabled = false;
            amountDisplay.text = "";
            return;
        }
        //Change Image to holding item
        itemDisplay.sprite = iDHandler.sprites[holdingItem];
        itemDisplay.enabled = true;
        //Change text to amount of the holding item
        amountDisplay.text = holdingItemAmount.ToString();
    }
}
