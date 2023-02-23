using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile
{
    public Sprite sprite;

    public Vector2Int loc;

    public Tile(Vector2Int loc, Sprite sprite)
    {
        this.loc = loc;
        this.sprite = sprite;
    }

}
