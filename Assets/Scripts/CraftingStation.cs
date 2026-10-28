using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CraftingStation : MonoBehaviour
{
    GameHandler gameHandler;

    private void Start()
    {
        gameHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<GameHandler>();
    }

    //Function called when interacting with the crafting station
    public void Interact()
    {
        gameHandler.ToggleUI("Crafting");
        gameHandler.GetComponent<CraftingHandler>().SetSelectedRecipe(null);
        gameHandler.GetComponent<CraftingHandler>().UpdateUI();
    } 
}
