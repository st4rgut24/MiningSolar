using System.Collections;
using System.Collections.Generic;
using Codice.CM.WorkspaceServer;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.WSA;

public class Draw : Singleton<Draw>
{
    [SerializeField]
    private Tilemap PlayerTilemap;

    [SerializeField]
    private Tilemap WeatherTilemap;

    [SerializeField]
    private Tilemap LandTilemap;

    [SerializeField]
    private Tilemap HighlightTilemap;


    [SerializeField]
    private Sprite LandSprite;


    [SerializeField]
    private Sprite HighlightSprite;

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

    public void highlightTiles(List<Vector2Int> tileLocs)
    {
        tileLocs.ForEach((loc) =>
        {
            Debug.Log("highlighted tile at " + loc);
            GameTile tile = new HighlightTile(loc, HighlightSprite);
            drawTile(tile);
        });
    }

    public void removeTiles(List<Vector2Int> tileLocs)
    {
        tileLocs.ForEach((loc) =>
        {
            GameTile tile = new GameTile(loc, null, GameTile.GenericType.None);
            Tilemap targetTilemap = getTilemap(tile);
            targetTilemap.SetTile(tile.loc, null);
        });
    }

    /// <summary>
    /// Draw a tile on the gameboard
    /// </summary>
    /// <param name="gameTile">The tile info</param>
    public void drawTile(GameTile gameTile)
    {
        Tilemap targetTilemap = getTilemap(gameTile);
        if (targetTilemap != null)
        {
            Debug.Log("get tile " + gameTile.loc);
            targetTilemap.SetTile(gameTile.loc, gameTile.getTile());
        }
    }

    public Tilemap getTilemap(GameTile gameTile)
    {
        switch (gameTile.genericType)
        {
            case GameTile.GenericType.Weather:
                return WeatherTilemap;
            case GameTile.GenericType.Land:
                return LandTilemap;
            case GameTile.GenericType.Player:
                return PlayerTilemap;
            case GameTile.GenericType.Highlight:
                return HighlightTilemap;
            default:
                throw new System.Exception("Not a valid tilemap " + gameTile.genericType);
        }
    }
}
