using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe", menuName = "Crafting Recipe")]
public class Recipe : ScriptableObject
{
    public int iD;

    //Items needed to craft the recipe
    public ItemHandler[] ingredients;
    public int[] amount;
}
