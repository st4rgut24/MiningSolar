using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/// <summary>
/// A object representing a tile in the map 
/// </summary>
public class PlayerTile : GameTile
{
    public IPlayer player;
    public Equipment.Type type;
    Equipment equipment;


    /// <summary>
    /// Create a tile belonging to a player
    /// </summary>
    /// <param name="player">A human player or bot</param>
    /// <param name="tileType">Type of tile</param>
    /// <param name="loc">location of tile</param>
    /// <param name="equipment">equipmnent located on tile if any</param>
    /// <param name="sprite">picture tile overlaid on land</param>
    public PlayerTile(IPlayer player, Equipment.Type tileType, Vector2Int loc, Equipment equipment = null, Sprite sprite = null) : base(loc, sprite, GenericType.Player)
    {
        this.player = player;
        this.type = tileType;
        this.equipment = equipment;
    }

    /// <summary>
    /// Place something on the tile
    /// </summary>
    public void setEquipment()
    {

    }
}
