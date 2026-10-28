using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Furnace : MonoBehaviour
{
    public int currentFuelLevel = 0;
    const int maxFuelLevel = 9;
    public ItemHandler smeltingOre = null;
    public Image ItemDisplay;
    public Sprite[] furnaceStates;

    Dictionary<int, int> smeltBehaviour = new Dictionary<int, int>();

    InventoryHandler inventoryHandler;
    IDHandler iDHandler;

    SpriteRenderer furnaceTexture;
    [HideInInspector]
    public Animator animator;

    private void Start()
    {
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();

        furnaceTexture = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        animator = gameObject.GetComponent<Animator>();

        //Setting up dictionary
        smeltBehaviour.Add(2, 16);
        smeltBehaviour.Add(5, 17);
    }

    public void InteractAsPlayer(ItemHandler item)
    {
        //Pulling smelted ore out of furnace
        if (smeltingOre && InventoryHandler.correctSlot(smeltingOre.iD) != -1 && InventoryHandler.inventory[InventoryHandler.correctSlot(smeltingOre.iD), 1] < InventoryHandler.maxStackSize && !animator.GetBool("smelt"))
        {
            inventoryHandler.AddItem(smeltingOre.iD, 1, InventoryHandler.correctSlot(smeltingOre.iD));
            smeltingOre = null;
            ItemDisplay.enabled = false;
        }

        else if (!item) return;
        //Putting coal in furnace, but more than furnace can take
        else if (item.iD == 7 && currentFuelLevel < maxFuelLevel && currentFuelLevel + InventoryHandler.CountOfSelectedItem() > maxFuelLevel)
        {
            inventoryHandler.RemoveItem(maxFuelLevel - currentFuelLevel, InventoryHandler.selectedSlot);
            currentFuelLevel = maxFuelLevel;
            furnaceTexture.sprite = furnaceStates[currentFuelLevel];
        }
        //Putting coal in furnace 
        else if (item.iD == 7 && currentFuelLevel < maxFuelLevel && currentFuelLevel + InventoryHandler.CountOfSelectedItem() <= maxFuelLevel)
        {
            currentFuelLevel += InventoryHandler.inventory[InventoryHandler.selectedSlot, 1];
            inventoryHandler.SetItem(0, 0, InventoryHandler.selectedSlot);
            furnaceTexture.sprite = furnaceStates[currentFuelLevel];
        }

        //Putting ore in furnace
        else if (!smeltingOre && CanSmelt(item) && currentFuelLevel > 0)
        {
            smeltingOre = item;
            inventoryHandler.RemoveItem(1, InventoryHandler.selectedSlot);
            StartCoroutine(SmeltOre());
        }
    }
    public void Interact(ItemHandler item)
    {
        //Putting one coal in furnace 
        if (item.iD == 7 && currentFuelLevel < maxFuelLevel)
        {
            currentFuelLevel++;
            furnaceTexture.sprite = furnaceStates[currentFuelLevel];
        }

        //Putting ore in furnace
        else if (!smeltingOre && CanSmelt(item) && currentFuelLevel > 0)
        {
            smeltingOre = item;
            StartCoroutine(SmeltOre());
        }
    }
    public bool CanInteract(ItemHandler item)
    {
        //Putting one coal in furnace 
        if (item.iD == 7 && currentFuelLevel < maxFuelLevel)
        {
            return true;
        }

        //Putting ore in furnace
        else if (!smeltingOre && CanSmelt(item) && currentFuelLevel > 0)
        {
            return true;
        }
        return false;
    }

    public IEnumerator SmeltOre()
    {
        animator.SetBool("smelt", true);
        yield return new WaitForSeconds(5f);
        smeltingOre = iDHandler.objects[smeltBehaviour[smeltingOre.iD]].GetComponent<ItemHandler>();
        currentFuelLevel--;
        furnaceTexture.sprite = furnaceStates[currentFuelLevel];
        //Update UI
        ItemDisplay.sprite = iDHandler.sprites[smeltingOre.iD];
        ItemDisplay.enabled = true;
        animator.SetBool("smelt", false);

        yield return null;
    }

    public bool CanSmelt(ItemHandler item)
    {
        foreach (var i in smeltBehaviour)
        {
            if (i.Key == item.iD && currentFuelLevel > 0) return true;
        }
        return false;
    }
}
