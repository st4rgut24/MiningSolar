using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Draw : Singleton<Draw>
{
    [SerializeField]
    private Tilemap PlayerTilemap;

    [SerializeField]
    private Tilemap WeatherTilemap;

    [SerializeField]
    private Tilemap LandTilemap;

    [SerializeField]
    private Sprite LandSprite;

    protected override void Awake()
    {
        base.Awake();
    }

    /// <summary>
    /// Draw a land tile
    /// </summary>
    /// <param name="landTileLoc">Location of the land tile</param>
    public void drawLandTile(Vector2Int landTileLoc)
    {
        LandTile landTile = new LandTile(landTileLoc, LandSprite);
        drawTile(landTile);
    }

    /// <summary>
    /// Draw a tile on the gameboard
    /// </summary>
    /// <param name="gameTile">The tile info</param>
    public void drawTile(GameTile gameTile)
    {
        Tilemap targetTilemap;

        switch (gameTile.genericType)
        {
            case GameTile.GenericType.Weather:
                targetTilemap= WeatherTilemap;
                break;
            case GameTile.GenericType.Land:
                targetTilemap= LandTilemap;
                break;
            case GameTile.GenericType.Player: 
                targetTilemap= PlayerTilemap;
                break;
            default: 
                throw new System.Exception("Not a valid tilemap " + gameTile.genericType);
        }
        if (targetTilemap != null)
        {
            Vector3Int tilemapLoc = new Vector3Int(gameTile.loc.x, gameTile.loc.y, 0);
            targetTilemap.SetTile(tilemapLoc, gameTile.getTile());
        }
    }
}
