using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IDHandler : MonoBehaviour
{
    public static string[] entity = new string[26]
    {
        //mineable objects at the top

        //0            //1                //2          //3    //4                  //5           //6                //7         //8     //9     
        "rock_object", "iron_ore_object", "iron_ore", "rock", "copper_ore_object", "copper_ore", "coal_ore_object", "coal_ore", "tree", "wood", 
        //10               //11             //12       //13   //14              //15            //16        //17            //18           //19
        "item_box_object", "item_box_item", "pickaxe", "axe", "furnace_object", "furnace_item", "iron_ingot", "copper_ingot", "belt_object", "belt_item",
        //20                //21              //22                       //23                     //24                   //25
        "connector_object", "connector_item", "crafting_station_object", "crafting_station_item", "object_miner_object", "object_miner_item"
    };

    public GameObject[] objects;
    [HideInInspector]
    public Sprite[] sprites;

    private void Awake()
    {
        LoadObjectTextures();
    }

    private void LoadObjectTextures()
    {
        sprites = new Sprite[objects.Length];
        for (int i = 0; i < objects.Length; i++)
        {
            sprites[i] = objects[i].transform.GetChild(0).GetComponent<SpriteRenderer>().sprite;
        }
    }

    public static string IDToName(int id)
    {
        if (id >= 0 && id <= entity.Length) return entity[id];
        else return "null";
    }
    public static int NameToID(string name)
    {
        for(int i = 0; i < entity.Length; i++)
        {
            if (entity[i] == name) return i;
        }
        return -1;
    }
}
