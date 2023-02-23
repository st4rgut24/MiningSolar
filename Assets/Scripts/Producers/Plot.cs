using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Scripts
{
    /**
     * A collection of adjacent land tiles belonging to a player that may be empty, solar or miner occupied
     */
    public class Plot
    {
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

        public Plot(IPlayer player, Vector2Int location, Plot prevAdjPlot)
        {
            tiles = new List<Vector2Int>() { location };
            miners = new List<Miner>();
            modules = new List<PVModule>();
            this.startingTile = location;
            this.adjacentPlotCount = 0;
            this.prevAdjPlot = prevAdjPlot;
            this.player = player;
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
        public BoundingBox getBoundingBox(int buffer)
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

        public void setStartingTile(Vector2Int startLoc)
        {
            this.tiles = new List<Vector2Int> { startLoc };

        }

        public List<Vector2Int> getTiles()
        {
            return this.tiles;
        }
    }

}