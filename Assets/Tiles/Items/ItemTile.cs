using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ItemTile : Tile {
    public Type type;
    public enum Type {
        Banana,
        Apple,
        IceFruit,
        FireFruit,
        ShockFruit,
    }
}