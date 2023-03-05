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
        [SerializeField]
        public int defaultCashReserves = 1000;

        [SerializeField]
        float decentralizedPct = .5f; // measures how decentralized plot placement is (1 is most decentralized, 0 is totally random)

        public int proximity; // how far away a plot should be placed from another's plot

        public Vector2Int defaultLoc;

        public List<Plot> plots { get; private set; }

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
        /// Get the eligible locations adjacent to an existing plot
        /// </summary>
        /// <returns>The randomly chosen eligible location</returns>
        private Vector2Int? getEligiblePlotLoc(int buffer, Plot plot)
        {
            BoundingBox boundingBox = plot.boundingBox;
            List<Vector2Int> adjLocList = boundingBox.getAdjPlotLocs();

            while (adjLocList.Count > 0)
            {
                int randLocIdx = Random.Range(0, adjLocList.Count);
                Vector2Int randLoc = adjLocList[randLocIdx];

                bool exists = Map.isPlayerTileInBufferZone(randLoc, plot.id);
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
        /// Get a random plot
        /// </summary>
        /// <returns>A random plot</returns>
        private Plot getRandPlot()
        {
            int randPlotIdx = Random.Range(0, plots.Count - 1);
            return plots[randPlotIdx];
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

            float rand = Random.Range(0, 1);
            Plot plot = rand < decentralizedPct ? this.getLeastConnectedPlot() : getRandPlot();

            while (plotLoc == null)
            {
                // expand eligible tiles at least one tile beyond the buffer zone
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
                newPlot = new BotPlot(bot, plotLoc.location, plotLoc.prevPlot, proximity);
            }
            else
            {
                newPlot = new Plot((Player)player, plotLoc.location, plotLoc.prevPlot, proximity);
            }
            if (newPlot.prevAdjPlot != null) // the first tile will be isolated
            {
                //Debug.Log("adj plot is at " + newPlot.prevAdjPlot.startingTile);
                newPlot.incrementAdjPlotCount();
                newPlot.prevAdjPlot.incrementAdjPlotCount(); // check that there is an adjacent Plot first
            }
            plots.Add(newPlot);
            return newPlot;
        }
    }

}
