using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Scripts;
using UnityEngine;
using UnityEngine.TestTools;

public class WeatherGenTest
{
    WeatherGenerator wg;

    static Vector2Int deltaLoc = Vector2Int.up;

    WeatherTile weatherTile;
    static Vector2Int tileLoc = new Vector2Int(3, 3);

    WeatherTile altWeatherTile;
    Vector2Int adjTileLoc = tileLoc + deltaLoc;

    [SetUp]
    public void SetUp()
    {
        var drawGo = new GameObject();
        drawGo.AddComponent<Draw>();

        var go = new GameObject();
        wg = go.AddComponent<WeatherGenerator>();

        weatherTile = new WeatherTile(tileLoc, wg.cloudSprite);
        Map.addWeatherTile(weatherTile);

        altWeatherTile= new WeatherTile(adjTileLoc, wg.cloudSprite);
        Map.addWeatherTile(altWeatherTile);

        var playerGo = new GameObject();
        Player player = playerGo.AddComponent<Player>();
        Vector2Int playerLoc = new Vector2Int(1, 1);
        Map.addPlayerTile(new PlayerTile(player, Equipment.Type.Miner, playerLoc, null));
    }

    /// <summary>
    /// The weather tile is updated when the weather pattern moves
    /// </summary>
    [Test]
    public void updatesTheWeatherTile()
    {
        WeatherTile formingStormTile = wg.updateWeatherPattern(deltaLoc, weatherTile);
        Assert.AreEqual(WeatherTile.TileType.Storm, formingStormTile.type);

        WeatherTile passingStormTile = wg.updateWeatherPattern(deltaLoc, formingStormTile);
        Assert.AreEqual(WeatherTile.TileType.Cloud, passingStormTile.type);
        Assert.AreEqual(WeatherTile.TileType.Cloud, formingStormTile.type);
    }

    /// <summary>
    /// The weather tile will wrap around the board when reaching edge of game area
    /// </summary>
    [Test]
    public void wrapsAcrossBoard()
    {
        for (int i = 0; i < Map.MAP_WIDTH; i++)
        {
            Vector2Int prevLoc = weatherTile.loc;
            weatherTile = wg.updateWeatherPattern(Vector2Int.right, weatherTile);
            Assert.IsNull(Map.getWeatherTile(prevLoc)); // removes the weather tile from its previous location
        }
        Assert.LessOrEqual(weatherTile.loc.x, Map.allPlotsBoundingBox.maxX);
    }
}
