using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryHandler : MonoBehaviour
{
    //X = ID, Y = Count
    public static int[,] inventory = new int[36, 2];
    public IDHandler iDHandler;
    public Image[] slots = new Image[36];
    public GameObject HotbarSlots;
    public GameObject BackpackSlots;
    public RectTransform Pointer;
    public Image HoldingItem;

    public static int maxStackSize = 64;
    public static int selectedSlot = 4;
    Vector2 holdingItem = new Vector2(0f, 0f);

    private void Start()
    {
        //Link all Slots to the array
        for (int i = 0; i < 9; i++)
        {
            slots[i] = HotbarSlots.transform.GetChild(i).GetComponent<Image>();
        }
        for (int i = 0, y = 9; i < 3; i++)
        {
            for (int j = 0; j < 9; j++, y++)
            {
                slots[y] = BackpackSlots.transform.GetChild(i).transform.GetChild(j).GetComponent<Image>();
            }
        }

        /*//TEST: Fill inventory with items
        for (int i = 0; i < 35; i++)
        {
            SetItem(0, 64, correctSlot(0));
        }
        SetItem(IDHandler.NameToID("iron_ore"), 63, 0);
        SetItem(IDHandler.NameToID("copper_ore"), 63, 1);
        SetItem(IDHandler.NameToID("coal_ore"), 63, 2);
        SetItem(IDHandler.NameToID("rock"), 63, 3);*/
        SetItem(IDHandler.NameToID("pickaxe"), 1, correctSlot(IDHandler.NameToID("pickaxe")));
        SetItem(IDHandler.NameToID("axe"), 1, correctSlot(IDHandler.NameToID("axe")));
        SetItem(IDHandler.NameToID("iron_ingot"), 64, correctSlot(IDHandler.NameToID("iron_ingot")));
        SetItem(IDHandler.NameToID("copper_ingot"), 64, correctSlot(IDHandler.NameToID("copper_ingot")));
        SetItem(IDHandler.NameToID("coal_ore"), 64, correctSlot(IDHandler.NameToID("coal_ore")));
    }

    private void Update()
    {
        //Get mouse position in the canvas UI and apply it to holding item
        HoldingItem.rectTransform.position = (Vector2)Input.mousePosition;

        //Scroll through the hotbar
        int scrollDir = (int)(Input.GetAxis("Mouse ScrollWheel") * 10f * -1f);
        if (scrollDir + selectedSlot >= 0 && scrollDir + selectedSlot <= 8) selectedSlot += scrollDir;
        else if (selectedSlot + scrollDir < 0) selectedSlot = 8;
        else if (selectedSlot + scrollDir > 8) selectedSlot = 0;
        //Switch hotbar slots by number on the top
        int number = GetNumberKey();
        if (number != -1) selectedSlot = number;

        Pointer.position = (Vector2)slots[selectedSlot].rectTransform.position + new Vector2(0f, 40f);
    }

    //Check if item with set id and specified amount is in player inventory
    public static bool IsInInventory(int id, int count)
    {
        int amountFound = 0;
        for (int i = 0; i < 36; i++)
        {
            //Look for matching item
            if (inventory[i, 0] == id) amountFound += inventory[i, 1];
        }
        if (amountFound >= count) return true;
        return false;
    }

    //Get amount of the item in the inventory
    public static int AmountInInventory(int id)
    {
        int amountFound = 0;
        for (int i = 0; i < 36; i++)
        {
            //Look for matching item
            if (inventory[i, 0] == id) amountFound += inventory[i, 1];
        }
        return amountFound;
    }

    public void SelectItem(int slot)
    {
        if (!BackpackSlots.transform.parent.gameObject.activeSelf) return;

        bool leftClick = Input.GetKey(KeyCode.Mouse0);
        bool rightClick = Input.GetKey(KeyCode.Mouse1);

        Vector2 temp = holdingItem;

        //Handling how the player can interact with the items in his inventory
        if (leftClick)
        {
            //Adding all holding items
            if((int)holdingItem.y > 0 && (int)holdingItem.x == inventory[slot, 0] && (int)holdingItem.y + inventory[slot, 1] <= maxStackSize)
            {
                AddItem((int)holdingItem.x, (int)holdingItem.y, slot);
                SetHoldingItem(0, 0);
            }
            //Adding as much holding items as possible
            else if ((int)holdingItem.y > 0 && (int)holdingItem.x == inventory[slot, 0] && ((int)holdingItem.y + inventory[slot, 1]) > maxStackSize && inventory[slot, 1] < maxStackSize)
            {
                SetHoldingItem((int)holdingItem.x, (int)holdingItem.y - maxAddableAmountOnSlot(slot, maxStackSize - inventory[slot, 1]));
                SetItem((int)holdingItem.x, maxStackSize, slot);
            }
            //Swapping items
            else if ((int)holdingItem.x != inventory[slot, 0] && inventory[slot, 1] != 0 || (int)holdingItem.y == 0 && inventory[slot, 1] != 0)
            {
                SetHoldingItem(inventory[slot, 0], inventory[slot, 1]);
                SetItem((int)temp.x, (int)temp.y, slot);
            }
            //Putting items in empty spot
            else if ((int)holdingItem.y > 0 && inventory[slot, 1] == 0)
            {
                SetItem((int)holdingItem.x, (int)holdingItem.y, slot);
                SetHoldingItem(0, 0);
            }
        }
        else
        {
            //Splitting items
            if((int)holdingItem.y == 0 && inventory[slot, 1] > 1)
            {
                SetHoldingItem(inventory[slot, 0], Mathf.RoundToInt(inventory[slot, 1] / 2));
                SetItem(inventory[slot, 0], inventory[slot, 1] - Mathf.RoundToInt(inventory[slot, 1] / 2), slot);
            }
            //Placing one item
            else if((int)holdingItem.y > 0 && inventory[slot, 1] == 0 || (int)holdingItem.y > 0 && inventory[slot, 0] == (int)holdingItem.x && inventory[slot, 1] < maxStackSize)
            {
                SetHoldingItem((int)holdingItem.x, (int)holdingItem.y - 1);
                AddItem((int)holdingItem.x, 1, slot);
            }
        }
    }

    public void AddItem(int id, int count, int slot)
    {
        if (slot < 0 || slot > 35) return;
        //Add the item
        inventory[slot, 0] = id;
        inventory[slot, 1] += count;

        //PrintInventory();
        //Debug.Log("Updated Slot " + slot + " to " + IDHandler.IDToName(id));

        UpdateSlotUI(slot);
    }

    public void SetItem(int id, int fixedCount, int slot)
    {
        if (slot < 0 || slot > 35) return;
        //Set the item
        inventory[slot, 0] = id;
        inventory[slot, 1] = fixedCount;

        UpdateSlotUI(slot);
    }

    public void RemoveItem(int amount, int slot)
    {
        if (slot < 0 || slot > 35) return;
        //Remove item on the specified slot completely
        inventory[slot, 1] -= amount;

        UpdateSlotUI(slot);
    }

    public static int maxAddableAmountOnSlot(int slot, int targetAmountToAdd)
    {
        if (inventory[slot, 1] + targetAmountToAdd > maxStackSize)
        {
            return maxStackSize - inventory[slot, 1];
        }
        else
        {
            return targetAmountToAdd;
        }
    }
    public static int maxSubtractableAmountOnSlot(int slot, int targetAmountToSubtract)
    {
        if (inventory[slot, 1] - targetAmountToSubtract < 0)
        {
            return inventory[slot, 1];
        }
        else
        {
            return targetAmountToSubtract;
        }
    }

    public static int correctSlot(int targetID)
    {
        //Check if there is a slot with the matching item
        for (int i = 0; i < 36; i++)
        {
            if (inventory[i, 0] == targetID && inventory[i, 1] < maxStackSize && inventory[i, 1] > 0) return i;
        }
        //Check for an empty slot
        for (int i = 0; i < 36; i++)
        {
            if (inventory[i, 1] == 0) return i;
        }
        return -1;
    }

    public static int matchingSlot(int targetID)
    {
        //Check inventory for an item with the same iD as the target
        for (int i = 0; i < 36; i++)
        {
            if (inventory[i, 0] == targetID) return i;
        }
        return -1;
    }

    public static int iDOfSelectedItem()
    {
        return inventory[selectedSlot, 0];
    }
    public static int CountOfSelectedItem()
    {
        return inventory[selectedSlot, 1];
    }

    void UpdateSlotUI(int slot)
    {
        if (inventory[slot, 1] > 0)
        {
            slots[slot].sprite = iDHandler.sprites[inventory[slot, 0]];
            slots[slot].enabled = true;
            slots[slot].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = inventory[slot, 1].ToString();
        }
        else
        {
            slots[slot].enabled = false;
            slots[slot].gameObject.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }

    public void SetHoldingItem(int id, int amount)
    {
        holdingItem = new Vector2(id, amount);
        if(holdingItem.y != 0)
        {
            HoldingItem.sprite = iDHandler.sprites[id];
            HoldingItem.enabled = true;
            HoldingItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = holdingItem.y.ToString();
        }
        else
        {
            HoldingItem.enabled = false;
            HoldingItem.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "";
        }
    }
    public void DropHoldingItem()
    {
        if ((int)holdingItem.y == 0) return;
        Transform player = GameObject.FindGameObjectWithTag("Player").transform;
        Vector2 targetPos = Vector2.ClampMagnitude((Camera.main.ScreenToWorldPoint(Input.mousePosition) - player.position), 1f) * 3.5f + (Vector2)player.position;
        //Right mouse, drop one item
        if (Input.GetKey(KeyCode.Mouse1) && (int)holdingItem.y > 0)
        {
            GameObject item = Instantiate(iDHandler.objects[(int)holdingItem.x], targetPos, Quaternion.identity);
            item.GetComponent<ItemHandler>().SetTargetVector((Vector2)item.transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            SetHoldingItem((int)holdingItem.x, (int)holdingItem.y - 1);
        }
        //Left mouse, drop all items (default option)
        else 
        {
            for (int i = 0; i < (int)holdingItem.y; i++)
            {
                GameObject item = Instantiate(iDHandler.objects[(int)holdingItem.x], targetPos, Quaternion.identity);
                item.GetComponent<ItemHandler>().SetTargetVector((Vector2)item.transform.position + new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)));
            }
            SetHoldingItem(0, 0);
        }
    }

    //Return ItemHandler of selected item
    public ItemHandler SelectedItem()
    {
        if (inventory[selectedSlot, 1] > 0) return iDHandler.objects[inventory[selectedSlot, 0]].GetComponent<ItemHandler>();
        else return null;
    }

    public static void PrintInventory()
    {
        Debug.Log("-- Iventory --");
        for (int i = 0; i < 9; i++)
        {
            Debug.Log("Slot " + i + ": " + IDHandler.IDToName(inventory[i, 0]) + " Amount: " + inventory[i, 1]);
        }
        Debug.Log("-- End of Inventory --");
    }

    int GetNumberKey()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) return 0;
        else if (Input.GetKeyDown(KeyCode.Alpha2)) return 1;
        else if (Input.GetKeyDown(KeyCode.Alpha3)) return 2;
        else if (Input.GetKeyDown(KeyCode.Alpha4)) return 3;
        else if (Input.GetKeyDown(KeyCode.Alpha5)) return 4;
        else if (Input.GetKeyDown(KeyCode.Alpha6)) return 5;
        else if (Input.GetKeyDown(KeyCode.Alpha7)) return 6;
        else if (Input.GetKeyDown(KeyCode.Alpha8)) return 7;
        else if (Input.GetKeyDown(KeyCode.Alpha9)) return 8;
        return -1;
    }
}
