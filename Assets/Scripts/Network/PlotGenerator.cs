using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts
{
    /**
     * Generates the Player at a random tile according to game configurations
     */
    public class PlotGenerator : Singleton<PlotGenerator>
    {
        public List<Plot> plots { get; private set; }
        public int proximity { get; private set; } // how far away a plot should be placed from another's plot
        public Vector2Int defaultLoc { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            plots = new List<Plot>();
        }

        public void initialize(int proximity, Vector2Int defaultLoc)
        {
            this.proximity = proximity;
            this.defaultLoc = defaultLoc;
        }

        /// <summary>
        /// Details about plot placement
        /// </summary>
        public class PlotLocation
        {
            public Vector2Int location { get; private set; }
            public Plot prevPlot { get; private set; }

            /// <summary>
            /// Plot Location Details
            /// </summary>
            /// <param name="location">tile location</param>
            /// <param name="prevPlot">plot this new plot is adjacent to</param>
            public PlotLocation(Vector2Int location, Plot prevPlot)
            {
                this.location = location;
                this.prevPlot = prevPlot;
            }
        }

        /// <summary>
        /// Get a tile location that borders this plot
        /// </summary>
        /// <param name="plot">The plot containing locations of its land</param>
        /// <returns>the bordering tile location that extends this plot</returns>
        public Vector2Int getAdjPlotLoc(Plot plot)
        {
            List<Vector2Int> fourDir = new List<Vector2Int>() { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
            List<Vector2Int>tiles = plot.getTiles();
            for (int i=0;i<tiles.Count;i++)
            {
                Vector2Int tile = tiles[i];
                for (int d = 0; d < fourDir.Count; d++)
                {
                    Vector2Int dir = fourDir[d];
                    Vector2Int adjTile = tile + dir;
                    bool isOccupied = Map.isPlayerTileExist(adjTile);
                    if (!isOccupied)
                    {
                        return adjTile;
                    }
                }
            }
            return GameManager.instance.NullableLoc; // returrn a falsy vector2int value indicating the plot is landlocked
        }

        /// <summary>
        /// Get locations that are within x tiles of a plot defined as a bounding box
        /// </summary>
        /// <param name="buffer">Amount of spacing separating plot tiles from new tile</param>
        /// <param name="plot">Least connected plot</param>
        /// <returns></returns>
        private List<Vector2Int> getAdjPlotLocsToBoundingBox(int buffer, Plot plot)
        {
            BoundingBox bb = plot.getBoundingBox(buffer);
            BoundingBox bufferBB = bb.bufferedBB;

            List<Vector2Int> adjPlotLocs = new List<Vector2Int>();
            // include tiles clockwise starting from the top side

            Vector2Int topLeftLoc = new Vector2Int(bufferBB.minX, bufferBB.maxY);
            for (int t = 0; t < bufferBB.width; t++)
            {
                adjPlotLocs.Add(new Vector2Int(topLeftLoc.x + t, topLeftLoc.y));
            }
            Vector2Int topRightLoc = new Vector2Int(bufferBB.maxX, bufferBB.maxY);
            for (int s = 1; s < bufferBB.height; s++)
            {
                adjPlotLocs.Add(new Vector2Int(topRightLoc.x, topRightLoc.y - s));
            }
            Vector2Int bottomRightLoc = new Vector2Int(bufferBB.maxX, bufferBB.minY);
            for (int d = 1; d<bufferBB.width; d++)
            {
                adjPlotLocs.Add(new Vector2Int(bottomRightLoc.x - d, bottomRightLoc.y));
            }
            Vector2Int bottomLeftLoc = new Vector2Int(bufferBB.minX, bufferBB.minY);
            for (int e=1;e<bufferBB.height - 1; e++)
            {
                adjPlotLocs.Add(new Vector2Int(bottomLeftLoc.x, bottomLeftLoc.y + e));
            }
            return adjPlotLocs;
        }

        /// <summary>
        /// Get the eligible locations adjacent to an existing plot
        /// </summary>
        /// <returns>The randomly chosen eligible location</returns>
        private Vector2Int? getEligiblePlotLoc(int buffer, Plot plot)
        {
            List<Vector2Int> adjLocList = this.getAdjPlotLocsToBoundingBox(buffer, plot);
            while (adjLocList.Count > 0)
            {
                int randLocIdx = Random.Range(0, adjLocList.Count);
                Vector2Int randLoc = adjLocList[randLocIdx];

                bool exists = Map.isPlayerTileExist(randLoc);
                if (exists)
                {
                    adjLocList.Remove(randLoc);
                }
                else
                {
                    return randLoc;
                }
            }
            return null;
        }

        /// <summary>
        /// Get the plot with the least connections
        /// </summary>
        /// <returns>plot</returns>
        public Plot getLeastConnectedPlot()
        {
            Plot minConnectedPlot = null;
            int minPlotCount = Int32.MaxValue;
            for (int i = 0; i < plots.Count; i++)
            {
                Plot plot = plots[i];
                int adjPlotCount = plot.getAdjPlotCount();
                if (adjPlotCount < minPlotCount)
                {
                    minConnectedPlot = plot;
                    minPlotCount = adjPlotCount;
                }
            }
            return minConnectedPlot;
        }

        /// <summary>
        /// Create a plot at a random location
        /// </summary>
        /// <returns>Details about plot placement</returns>
        public PlotLocation GetRandomPlotLocation()
        {
            Vector2Int? plotLoc = null;
            int buffer = proximity;

            if (plots.Count == 0)
            {
                return new PlotLocation(defaultLoc, null);
            }

            Plot plot = this.getLeastConnectedPlot();

            while (plotLoc == null)
            {
                plotLoc = this.getEligiblePlotLoc(buffer, plot);
                buffer += 1;
            }

            return new PlotLocation((Vector2Int)plotLoc, plot); // cast to avoid error of optional assignment
        }

        /// <summary>
        /// Creates a plot for a player in the map
        /// </summary>
        /// <param name="player">the player</param>
        /// <returns>
        /// location of the start tile
        /// </returns>
        public Plot GetPlot(IPlayer player)
        {
            Plot newPlot;
            PlotLocation plotLoc = GetRandomPlotLocation();
            if (player.isBot())
            {
                Bot bot = (Bot)player;
                newPlot = new BotPlot(bot, plotLoc.location, plotLoc.prevPlot);
            }
            else
            {
                newPlot = new Plot((Player)player, plotLoc.location, plotLoc.prevPlot);
            }
            if (newPlot.prevAdjPlot != null) // the first tile will be isolated
            {
                newPlot.incrementAdjPlotCount();
                newPlot.prevAdjPlot.incrementAdjPlotCount(); // check that there is an adjacent Plot first
            }
            plots.Add(newPlot);
            return newPlot;
        }
    }

}
