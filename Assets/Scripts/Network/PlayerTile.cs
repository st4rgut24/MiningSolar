using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/// <summary>
/// A object representing a tile in the map 
/// </summary>
public class PlayerTile : Tile
{
    public IPlayer player;
    public TileType type;
    Equipment equipment;

    public enum TileType
    {
        Miner,
        Solar,
        Empty
    }

    /// <summary>
    /// Create a tile belonging to a player
    /// </summary>
    /// <param name="player">A human player or bot</param>
    /// <param name="tileType">Type of tile</param>
    /// <param name="loc">location of tile</param>
    /// <param name="equipment">equipmnent located on tile if any</param>
    /// <param name="sprite">picture tile overlaid on land</param>
    public PlayerTile(IPlayer player, TileType tileType, Vector2Int loc, Equipment equipment = null, Sprite sprite = null) : base(loc, sprite)
    {
        this.player = player;
        this.type = tileType;
        this.equipment = equipment;
        this.sprite = sprite;
    }

    /// <summary>
    /// Place something on the tile
    /// </summary>
    public void setEquipment()
    {

    }
}
