using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

using Scripts;
using System;

public class PlotTest
{
    const int PROXIMITY = 1;
    Vector2Int defaultLoc = new Vector2Int(0, 0);

    IPlayer player1;
    IPlayer player2;
    IPlayer player3;

    PlotGenerator pg;

    [SetUp]
    public void SetUp()
    {
        var player1Go = new GameObject();
        var player2Go = new GameObject();
        var player3Go = new GameObject();

        player1 = player1Go.AddComponent<Player>();
        player2 = player2Go.AddComponent<Player>();
        player3 = player3Go.AddComponent<Player>();

        var plotGenGo = new GameObject();
        pg = plotGenGo.AddComponent<PlotGenerator>();
        pg.initialize(PROXIMITY, defaultLoc);
    }

    /// <summary>
    /// The first plot is in the correct default location
    /// </summary>
    [Test]
    public void FirstPlotLocation()
    {
        Plot firstPlot = pg.GetPlot(player3);
        Assert.AreEqual(firstPlot.startingTile, pg.defaultLoc);
    }

    /// <summary>
    /// The least connected plot has the correct number of connections after adding 3 plots
    /// </summary>
    [Test]
    public void PlotConnectionCount()
    {
        Plot firstPlot = pg.GetPlot(player1);
        Plot secondPlot = pg.GetPlot(player2); // after addition, both plots have 1 adjacent connection to each other
        Plot thirdPlot = pg.GetPlot(player3); // one pair of plots will have 2 connections, the last will still have 1
        Assert.AreEqual(2, firstPlot.getAdjPlotCount());
        Assert.AreEqual(1, secondPlot.getAdjPlotCount());
        Assert.AreEqual(1, thirdPlot.getAdjPlotCount());
    }

    /// <summary>
    /// Maintain the correct distance between new plots
    /// </summary>
    [Test]
    public void DistanceBetweenPlots()
    {
        for (int proximity = 1; proximity < 4; proximity++)
        {
            pg.initialize(proximity, defaultLoc);

            Plot plot = pg.GetPlot(player1);
            if (plot.prevAdjPlot == null)
            {
                Assert.AreEqual(plot.startingTile, defaultLoc);
            }
            else
            {
                int prevPlotBuffer = plot.prevAdjPlot.boundingBox.buffer;
                int xDist = Mathf.Abs(plot.startingTile.x - plot.prevAdjPlot.startingTile.x) - 1;
                int yDist = Mathf.Abs(plot.startingTile.y - plot.prevAdjPlot.startingTile.y) - 1;
                bool isDistCorrect = xDist == prevPlotBuffer || yDist == prevPlotBuffer;
                Assert.IsTrue(isDistCorrect);
            }
    }
}

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator InitPlayerWithEnumeratorPasses()
    {
        // Use the Assert class to test conditions.
        // Use yield to skip a frame.
        yield return null;
    }
}
