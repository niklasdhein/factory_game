using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameHandler : MonoBehaviour
{
    [Range (30, 360)]
    public int targetFrameRate = 60;
    public GameObject QuitButton;
    public GameObject[] UIScreens;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = targetFrameRate;   
    }

    private void Update()
    {
        //Escape Button
        if (Input.GetKeyDown(KeyCode.Escape)) QuitButton.SetActive(!QuitButton.activeSelf);

        //Handle keybinds specified for UI
        //Toggle backpack UI, and drop the item the player is holding 
        if (Input.GetKeyDown(KeyCode.E) && OnlyActive(GetUIScreen("Backpack")))
        {
            ToggleUI("Backpack");
            gameObject.GetComponent<InventoryHandler>().DropHoldingItem();
        }
        //Toggle crafting UI 
        else if (Input.GetKeyDown(KeyCode.E) && OnlyActive(GetUIScreen("Crafting")))
        {
            ToggleUI("Crafting");
        }
    }

    public void Quit()
    {
        Application.Quit(0);
    }

    //Check if any UI Screen is currently active
    public bool AnyUIActive()
    {
        foreach(GameObject obj in UIScreens)
        {
            if (obj.activeSelf) return true;
        }
        return false;
    }

    //Check if all UIs are closed except of this one 
    public bool OnlyActive(GameObject targetObj)
    {
        foreach(GameObject obj in UIScreens)
        {
            if (obj.activeSelf && obj != targetObj) return false;
        }
        return true;
    }

    //Open or close a UI window
    public void ToggleUI(string UIName)
    {
        foreach(GameObject obj in UIScreens)
        {
            if (obj.name == UIName) obj.SetActive(!obj.activeSelf);
        }
    }

    //Get a UI object by its name
    public GameObject GetUIScreen(string UIName)
    {
        foreach (GameObject obj in UIScreens)
        {
            if (obj.name == UIName) return obj;
        }
        return null;
    }
}
