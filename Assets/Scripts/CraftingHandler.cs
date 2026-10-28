using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingHandler : MonoBehaviour
{
    public Recipe[] recipes;
    public GameObject RecipePrefab;
    public GameObject IngredientPrefab;

    Recipe selectedRecipe;
    Image selectedRecipeImage;

    GameHandler gameHandler;
    IDHandler iDHandler;
    InventoryHandler inventoryHandler;

    private void Start()
    {
        //Get references
        gameHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<GameHandler>();
        iDHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<IDHandler>();
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        selectedRecipeImage = gameHandler.GetUIScreen("Crafting").transform.GetChild(4).GetComponent<Image>();

        //Link the UI Button for crafting to the craft function
        gameHandler.GetUIScreen("Crafting").transform.GetChild(3).GetComponent<Button>().onClick.AddListener(CraftSelectedRecipe);

        //Load all recipes when the game loads
        LoadRecipes();
    }

    public void SetSelectedRecipe(Recipe recipe)
    {
        selectedRecipe = recipe;
        //Disable display and change selected recipe to null
        //when no recipe should be shown
        if (!recipe) selectedRecipeImage.enabled = false;
        else
        {
            //Load in selected recipe and change display
            selectedRecipeImage.sprite = iDHandler.sprites[recipe.iD];
            selectedRecipeImage.enabled = true;
        }
        UpdateUI();
    }

    public void CraftSelectedRecipe()
    {
        if (!selectedRecipe) return;

        //Check if all materials neccessary are in the inventory
        for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
        {
            //Return if one item is not in needed amount in inventory
            if (!InventoryHandler.IsInInventory(selectedRecipe.ingredients[i].iD, selectedRecipe.amount[i]))
            {
                Debug.Log("Cant craft item, not enough ressources!");
                return;
            }
        }

        //Return if theres no space for the crafted item
        if (InventoryHandler.correctSlot(selectedRecipe.iD) == -1) return;

        //Remove the items needed to craft from inventory
        for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
        {
            int targetAmountToSubstract = selectedRecipe.amount[i];
            //Enough items needed on one slot
            if (InventoryHandler.inventory[InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD), 1] >= targetAmountToSubstract)
            {
                inventoryHandler.RemoveItem(targetAmountToSubstract, InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD));
            }

            //One slot is not enough, use multiple slots
            else
            {
                int amountFound = 0;
                while (amountFound != targetAmountToSubstract)
                {
                    if (InventoryHandler.inventory[InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD), 1] <= targetAmountToSubstract - amountFound)
                    {
                        amountFound += InventoryHandler.inventory[InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD), 1];
                        inventoryHandler.SetItem(0, 0, InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD));
                    }
                    else
                    {
                        inventoryHandler.RemoveItem(targetAmountToSubstract - amountFound, InventoryHandler.matchingSlot(selectedRecipe.ingredients[i].iD));
                        amountFound = targetAmountToSubstract;
                    }
                }
            }
        }

        //Add the crafted item to player inventory
        inventoryHandler.AddItem(selectedRecipe.iD, 1, InventoryHandler.correctSlot(selectedRecipe.iD));

        UpdateUI();

        Debug.Log("The player crafted " + selectedRecipe.name);
    }

    public void UpdateUI()
    {
        //Remove the ingredients of the last selected recipe
        for (int i = gameHandler.GetUIScreen("Crafting").transform.GetChild(2).childCount - 1; i >= 0; i--)
        {
            Destroy(gameHandler.GetUIScreen("Crafting").transform.GetChild(2).GetChild(i).gameObject);
        }

        if (!selectedRecipe) return;

        //Load all needed ingredients
        for (int i = 0; i < selectedRecipe.ingredients.Length; i++)
        {
            //Instantiate
            GameObject obj = Instantiate(IngredientPrefab, gameHandler.GetUIScreen("Crafting").transform.GetChild(2));
            //Change Text
            TextMeshProUGUI text = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            text.text = InventoryHandler.AmountInInventory(selectedRecipe.ingredients[i].iD).ToString() + "/" + selectedRecipe.amount[i].ToString();
            //Change GFX
            if (!InventoryHandler.IsInInventory(selectedRecipe.ingredients[i].iD, selectedRecipe.amount[i])) text.color = Color.red;
            else text.color = Color.green;
            obj.GetComponent<Image>().sprite = iDHandler.sprites[selectedRecipe.ingredients[i].iD];
        }
    }

    void LoadRecipes()
    {
        //Spawn all recipes and add them to the UI
        foreach (Recipe rec in recipes)
        {
            //Spawn Instance
            GameObject obj = Instantiate(RecipePrefab, gameHandler.GetUIScreen("Crafting").transform.GetChild(1));
            //Link the buttons up to the craft function that the player can actually interact with them
            obj.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(delegate { SetSelectedRecipe(rec); });
            //Set sprite to crafting recipe item
            obj.GetComponent<Image>().sprite = iDHandler.sprites[rec.iD];
        }
    }
}