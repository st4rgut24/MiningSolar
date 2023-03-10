using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Scripts { }
public class GameTile
{
    public Vector3Int loc;

    public Tile mapTile { get; private set; }

    public GenericType genericType { get; private set; }

    public enum GenericType
    {
        Land,
        Player,
        Weather,
        Highlight,
        None
    }

    public UnityEngine.Tilemaps.Tile getTile()
    {
        var tile = new UnityEngine.Tilemaps.Tile();
        tile.sprite = mapTile.sprite;
        return tile;
    }

    public GameTile(Vector2Int loc, Sprite sprite, GenericType type)
    {
        this.loc = loc.toVector3Int();
        mapTile = new Tile(loc, sprite);
        mapTile.sprite = sprite;
        this.genericType = type;
    }

}
