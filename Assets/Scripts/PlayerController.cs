using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public float moveSpeed;
    private float xDir, yDir;
    public Transform mainCam;
    public float camSpeed;
    public LayerMask itemLayer;
    public static int strength = 3;

    InventoryHandler inventoryHandler;
    Animator animator;

    private void Start()
    {
        //Get Inventory and IDHandler Reference
        inventoryHandler = GameObject.FindGameObjectWithTag("GameHandler").GetComponent<InventoryHandler>();
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void Update()
    {
        //Get Player Movement Input
        xDir = Input.GetAxisRaw("Horizontal");
        yDir = Input.GetAxisRaw("Vertical");

        //Set animation values
        animator.SetInteger("xDir", Mathf.RoundToInt(xDir));
        animator.SetInteger("yDir", Mathf.RoundToInt(yDir));

        //Let the camera follow the player
        Vector2 cDir = transform.position - mainCam.position;
        mainCam.transform.position = mainCam.position + new Vector3(cDir.x * Time.deltaTime * camSpeed, cDir.y * Time.deltaTime * camSpeed, 0);
    }

    private void FixedUpdate()
    {
        //Move the player according to his input
        Move();

        //Check for items nearby to pick up
        Collider2D[] collision = Physics2D.OverlapCircleAll((Vector2)transform.position, 1.414f, itemLayer);
        if (collision != null) PickUpItem(collision);
    }

    private void Move()
    {
        if (xDir != 0 && yDir != 0)
        {
            rb.MovePosition(rb.position + new Vector2(xDir * 0.75f, yDir * 0.75f) * moveSpeed * Time.fixedDeltaTime);
            return;
        }
        rb.MovePosition(rb.position + new Vector2(xDir, yDir) * moveSpeed * Time.fixedDeltaTime);
    }

    private void PickUpItem(Collider2D[] items)
    {
        foreach (Collider2D i in items)
        {
            ItemHandler item = i.gameObject.GetComponent<ItemHandler>(); 
            int targetSlot = InventoryHandler.correctSlot(item.iD);
            if (targetSlot != -1 && item.allowPickup)
            {
                //Add item to inventory 
                inventoryHandler.AddItem(item.GetComponent<ItemHandler>().iD, 1, targetSlot);

                //Move item into players inventory
                item.gameObject.GetComponent<BoxCollider2D>().enabled = false;
                item.SetTargetTransform(transform);
                item.destroyWhenTargetReached = true;
                item.speed = 7;
            }
        }
    }
}
