using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ObjectHandler : MonoBehaviour
{
    public int iD;
    public int health = 3;
    public string requiredTool;
    public GameObject[] objectDrops;
    public int rangeMinDropAmount = 1, rangeMaxDropAmount;
    public GameObject[] interactableItems;
    public bool allowAllItemsToInteract;

    [HideInInspector]
    public bool destructable = true;

    InventoryHandler inventoryHandler;

    private void Start()
    {
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
    }

    //Event called when the player interacts with this object
    public void Interact()
    {
        //Determine whether the player left or right-clicked this object
        bool leftClick = Input.GetKey(KeyCode.Mouse0);
        bool rightClick = Input.GetKey(KeyCode.Mouse1);

        if (leftClick)
        {
            //Destroy object if player is using a pickaxe
            if (objectDrops != null && (IDHandler.IDToName(InventoryHandler.iDOfSelectedItem()) == requiredTool || requiredTool == ""))
            {
                health -= PlayerController.strength;
                if (health <= 0 && destructable)
                {
                    DropItems(Mathf.RoundToInt(Random.Range(rangeMinDropAmount, rangeMaxDropAmount)));
                    DestroyObject();
                }
            }
        }
        //Right click
        else
        {
            //Check if the players selected item is interactable with this object (chests, furnaces, etc..)
            if (allowAllItemsToInteract || IsItemUsableWithThisObject(inventoryHandler.SelectedItem()))
            {
                switch (iD)
                {
                    //item_box_object
                    case 10:
                        ItemBox itemBox = gameObject.GetComponent<ItemBox>();
                        itemBox.Interact();
                        break;
                    //furnace
                    case 14:
                        Furnace furnace = gameObject.GetComponent<Furnace>();
                        furnace.InteractAsPlayer(inventoryHandler.SelectedItem());
                        break;
                    //item_belt_object
                    case 18:
                        Belt belt = gameObject.GetComponent<Belt>();
                        belt.ClickOnBelt();
                        break;
                    //connector_object
                    case 20:
                        Connector connector = gameObject.GetComponent<Connector>();
                        connector.Interact();
                        break;
                    case 22:
                        //crafting_station
                        CraftingStation craftingStation = gameObject.GetComponent<CraftingStation>();
                        craftingStation.Interact();
                        break;
                    case 24:
                        //object_miner
                        ObjectMiner miner = gameObject.GetComponent<ObjectMiner>();
                        miner.Interact();
                        break;
                }
            }
        }
    }




    bool IsItemUsableWithThisObject(ItemHandler item)
    {
        if (interactableItems != null && item != null)
        {
            foreach (GameObject obj in interactableItems)
            {
                if (item.gameObject == obj) return true;
            }
            return false;
        }
        else return false;
    }

    void DropItems(int count)
    {
        for(int i = 0; i < count; i++)
        {
            foreach (GameObject obj in objectDrops)
            { 
                Vector2 pos = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f));
                ItemHandler item = Instantiate(obj, (Vector2)transform.position, Quaternion.identity).GetComponent<ItemHandler>();
                item.SetTargetVector(pos + (Vector2)transform.position);
                item.destroyWhenTargetReached = false;
            }
        }
    }

    void DestroyObject()
    {
        Destroy(gameObject);
    }
}
