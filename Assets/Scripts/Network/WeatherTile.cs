using Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherTile : Tile
{
    public TileType type { get; private set; }
    public int cloudCount;

    public enum TileType
    {
        Cloud,
        Storm,
        HellRain
    }

    /// <summary>
    /// Add a cloud to this tile and update the tile's type
    /// </summary>
    /// <param name="deltaCloud">Change in cloud count</param>
    public void updateCloudCount(int deltaCloud)
    {
        this.cloudCount++;
    }

    /// <summary>
    /// Update the cloud type
    /// </summary>
    /// <param name="type">cloud type</param>
    public void updateCloudType()
    {
        if (cloudCount == 1)
        {
            type = TileType.Cloud;
        }
        else if (cloudCount == 2)
        {
            type = TileType.Storm;
        }
        else
        {
            type = TileType.HellRain;
        }
    }

    public WeatherTile(Vector2Int loc, Sprite sprite) : base(loc, sprite)
    {
        this.type = TileType.Cloud;
        this.cloudCount = 1;
    }
}
