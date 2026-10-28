using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static Vector2 selectorPos;
    public Transform selector;
    public float snippingStrength = 0.5f;
    public Camera cam;
    public LayerMask objLayer;
    
    Transform playerTransform;
    InventoryHandler inventoryHandler;
    GameHandler gameHandler;

    public static bool selectorOnObject;

    private void Start()
    {
        //Get references
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        gameHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<GameHandler>();
    }

    private void Update()
    {
        //Do not do anything if any UI screen is opened
        if (gameHandler.AnyUIActive()) return;

        //Get mouse position and calculate its target tile
        Vector3 mousePosRaw = cam.ScreenToWorldPoint(Input.mousePosition);
        selectorPos = new Vector2(Mathf.RoundToInt(mousePosRaw.x), Mathf.RoundToInt(mousePosRaw.y));

        //Move the selector to the target
        selector.position = snippingStrength * Time.deltaTime * ((Vector2)selectorPos - (Vector2)selector.position) + (Vector2)selector.position;

        //Get current object the selector hovers over
        GameObject obj = GetObject();

        //Interact with object on selector pos (left click event)
        if (Input.GetKeyDown(KeyCode.Mouse0)) InteractWithObject(obj);
        //Check if player can place the holding item
        else if (Input.GetKeyDown(KeyCode.Mouse1) && !selectorOnObject && InventoryHandler.CountOfSelectedItem() > 0 && inventoryHandler.SelectedItem().isPlacable) inventoryHandler.SelectedItem().Place();
        //Interact with object on selector pos (right click event)
        else if (Input.GetKeyDown(KeyCode.Mouse1)) InteractWithObject(obj);
    }

    GameObject GetObject()
    {
        Collider2D _object = Physics2D.OverlapCircle(selector.position, 0.01f, objLayer);
        if (_object)
        {
            selectorOnObject = true;
            return _object.gameObject;
        }
        else
        {
            selectorOnObject = false;
            return null;
        }
    }

    void InteractWithObject(GameObject targetObject)
    {
        //Use the selected Item
        if (!targetObject || Vector2.Distance(targetObject.transform.position, playerTransform.position) > 4) return;
        //Debug.Log("Interacting with " + IDHandler.IDToName(targetObject.GetComponent<ObjectHandler>().iD));
        targetObject.GetComponent<ObjectHandler>().Interact();
    }
}
