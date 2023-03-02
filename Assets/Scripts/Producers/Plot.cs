using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Graphs;
using UnityEngine;

namespace Scripts
{
    /**
     * A collection of adjacent land tiles belonging to a player that may be empty, solar or miner occupied
     */
    public class Plot
    {
        public string id { get; private set; }

        Scheduler scheduler;

        public BoundingBox boundingBox { get; private set; }
        public Vector2Int startingTile { get; private set; }
        public Plot prevAdjPlot { get; private set; } // plot set adjacent to on creation (if any)

        public List<Miner> miners;
        public List<PVModule> modules;

        protected IPlayer player;          // player plot belongs to

        protected List<Vector2Int> tiles; // location of tiles making up the plot

        // received energy in wattHours
        protected int selfProducedEnergy;
        protected int importedEnergy;

        protected float energyReserves;
        protected float cashReserves;

        protected int adjacentPlotCount; // count of nearby plots belonging to other players

        protected float bitcoinProduction;

        private int buffer;

        public Plot(IPlayer player, Vector2Int location, Plot prevAdjPlot, int buffer)
        {
            tiles = new List<Vector2Int>() { location };
            miners = new List<Miner>();
            modules = new List<PVModule>();
            this.startingTile = location;
            this.adjacentPlotCount = 0;
            this.prevAdjPlot = prevAdjPlot;
            this.player = player;
            this.buffer = buffer;
            id = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// Change the cash reserves of this plot
        /// </summary>
        /// <param name="deltaCash">The change in the cash reserves</param>
        public void changeCashReserves(float deltaCash)
        {
            cashReserves += deltaCash;
        } 

        /// <summary>
        /// Change the energy reserves of this plot
        /// </summary>
        /// <param name="deltaWatts"></param>
        public void changeEnergyReserves(int deltaWatts)
        {
            energyReserves += deltaWatts;
        }

        /// <summary>
        /// Change the amount of self produced energy
        /// </summary>
        /// <param name="deltaEnergy">The change in self produced energy</param>
        public void changeSelfProducedEnergy(int deltaEnergy)
        {
            selfProducedEnergy += deltaEnergy;
            changeEnergyReserves(deltaEnergy);
        }

        /// <summary>
        /// Amount of energy imported and self produced
        /// </summary>
        /// <returns>Total energy used on plot</returns>
        public int getTotalEnergyProduction()
        {
            return selfProducedEnergy + importedEnergy;
        }

        /// <summary>
        /// Add a pv module to the plot
        /// </summary>
        /// <param name="module">new module</param>
        public void addModule(PVModule module)
        {
            modules.Add(module);
        }

        /// <summary> 
        /// Imported energy measured in watt hours the same way self produced energy is
        /// </summary>
        /// <param name="importedWattHours"></param>
        public void importEnergy(int importedWattHours)
        {
            importedEnergy += importedWattHours;
            changeEnergyReserves(importedWattHours);
        }

        /// <summary>
        /// Add a miner to the plot
        /// </summary>
        /// <param name="miner">The new miner</param>
        public void addMiner(Miner miner)
        {
            miners.Add(miner);
            RewardGenerator.instance.addMiner(miner);
        }

        /// <summary>
        /// Get hash power of all miners on this plot
        /// </summary>
        /// <returns>Total hash power</returns>
        public float getHashPower()
        {
            float hashPow = 0;
            miners.ForEach((miner) => { hashPow += miner.hashingPower; });
            return hashPow;
        }

        public string getOwnerId()
        {
            return player.id;
        }

        public void incrementAdjPlotCount()
        {
            this.adjacentPlotCount++;
        }

        /// <summary>
        /// Get the convex shape of a player to define the edges of their plot
        /// </summary>
        /// <param name="buffer"> the buffer surrounding the box defined as the space where no plots belonging to other
        /// players can intrude</param>
        /// <returns>a surrouding rectangle enclosing the player's tiles</returns>
        private BoundingBox createBoundingBox(int buffer)
        {
            int maxXTile = Int32.MinValue;
            int minXTile = Int32.MaxValue;

            int maxYTile = Int32.MinValue;
            int minYTile = Int32.MaxValue;
            for (int t = 0; t < tiles.Count; t++)
            {
                Vector2Int tileLoc = tiles[t];
                if (tileLoc.x < minXTile) { minXTile = tileLoc.x; }
                if (tileLoc.y < minYTile) { minYTile = tileLoc.y; }
                if (tileLoc.x > maxXTile) { maxXTile = tileLoc.x; }
                if (tileLoc.y > maxYTile) { maxYTile = tileLoc.y; }
            }
            return new BoundingBox(minXTile, minYTile, maxXTile, maxYTile, buffer);
        }

        public List<Miner> getMiners()
        {
            return miners;
        }

        public int getAdjPlotCount()
        {
            return adjacentPlotCount;
        }

        /// <summary>
        /// Update the plot
        /// </summary>
        /// <param name="tileLoc">location of tile to add</param>
        public void addTile(Vector2Int tileLoc)
        {
            this.tiles.Add(tileLoc);
        }

        /// <summary>
        /// Get a tile location that borders this plot
        /// </summary>
        /// <param name="plot">The plot containing locations of its land</param>
        /// <returns>the bordering tile location that extends this plot</returns>
        public Vector2Int getAdjPlotLoc()
        {
            List<Vector2Int> fourDir = new List<Vector2Int>() { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
            List<Vector2Int> tiles = getTiles();
            for (int i = 0; i < tiles.Count; i++)
            {
                Vector2Int tile = tiles[i];
                for (int d = 0; d < fourDir.Count; d++)
                {
                    Vector2Int dir = fourDir[d];
                    Vector2Int adjTile = tile + dir;
                    bool isBlocked = Map.isPlayerTileInBufferZone(adjTile, id) || Map.isTileOccupied(adjTile);
                    if (!isBlocked)
                    {
                        return adjTile;
                    }
                }
            }
            return GameManager.instance.NullableLoc; // returrn a falsy vector2int value indicating the plot is landlocked
        }

        /// <summary>
        /// Remove a tile at location
        /// </summary>
        /// <param name="tileLoc">the locatin of tile to be removed</param>
        private void removeTile(Vector2Int tileLoc)
        {
            if (boundingBox != null)
            {
                // remove buffered locs from the map
            }
            tiles.Remove(tileLoc);
            boundingBox = createBoundingBox(buffer);
        }

        public List<Vector2Int> getTiles()
        {
            return this.tiles;
        }
    }

}