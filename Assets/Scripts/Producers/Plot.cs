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
        protected int cumSelfProducedEnergy = 0;
        protected int cumImportedEnergy = 0;

        public float cumBitcoinProduced = 0;

        protected float energyReserves;
        protected float cashReserves;

        protected int adjacentPlotCount; // count of nearby plots belonging to other players


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

            cashReserves = PlotGenerator.instance.defaultCashReserves;
            this.boundingBox = createBoundingBox(buffer, id);
        }

        /// <summary>
        /// Change the cash reserves of this plot
        /// </summary>
        /// <param name="deltaCash">The change in the cash reserves</param>
        public void changeCashReserves(float deltaCash)
        {
            if (cashReserves + deltaCash < 0)
            {
                throw new Exception("Can't have a negative balance");
            }
            cashReserves += deltaCash;
        }

        /// <summary>
        /// Change the amount of bitcoin produced from this plot
        /// </summary>
        /// <param name="bitcoin"></param>
        public void changeBitcoinProd(float deltaBitcoin)
        {
            if (deltaBitcoin < 0)
            {
                throw new Exception("A negative bitcoin production is not allowed");
            }
            this.cumBitcoinProduced += deltaBitcoin;
        }

        /// <summary>
        /// Tally the cumulative energy produced on this plot
        /// </summary>
        /// <param name="wattsAdded">Number of watts added</param>
        public void addSelfGeneratedEnergy(int wattsAdded)
        {
            cumSelfProducedEnergy += wattsAdded;
        }

        /// <summary>
        /// Change the amount of self produced energy
        /// </summary>
        /// <param name="deltaEnergy">The change in self produced energy</param>
        public bool changeEnergyReserves(int deltaEnergy)
        {
            // cannot spend more energy than are in the reserves
            if (deltaEnergy + energyReserves < 0)
            {
                return false;
            }
            else
            {
                energyReserves += deltaEnergy;
                Debug.Log("updating energy reserves to " + energyReserves);
                return true;
            }
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
            cumImportedEnergy += importedWattHours;
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
        /// Get active hash power of all miners on this plot
        /// </summary>
        /// <returns>Total hash power</returns>
        public float getHashPower()
        {
            float hashPow = 0;
            miners.ForEach((miner) => { hashPow += miner.activeHashingPower; });
            return hashPow;
        }

        /// <summary>
        /// Get the active watt hours of all pv modules on this plot
        /// </summary>
        /// <returns>total watt hours</returns>
        public int getEnergyProd()
        {
            int totalActivePower = 0;
            modules.ForEach((module) => { totalActivePower += module.activeWattHours; });
            return totalActivePower;
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
        /// <param name="id"/> id of the bounding box matches the plot's<param>
        /// players can intrude</param>
        /// <returns>a surrouding rectangle enclosing the player's tiles</returns>
        private BoundingBox createBoundingBox(int buffer, string id)
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
            return new BoundingBox(minXTile, minYTile, maxXTile, maxYTile, buffer, id);
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
        /// Get all tile locs border this plot that are eligible for a new tile
        /// </summary>
        /// <returns>the list of eligible tile locations</returns>
        public List<Vector2Int> getAdjPlotLocs()
        {
            List<Vector2Int> fourDir = new List<Vector2Int>() { Vector2Int.up, Vector2Int.left, Vector2Int.down, Vector2Int.right };
            List<Vector2Int> tiles = getTiles();
            List<Vector2Int> eligibleTiles = new List<Vector2Int>();

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
                        eligibleTiles.Add(adjTile);
                    }
                }
            }
            return eligibleTiles;
        }

        /// <summary>
        /// Get a tile location that borders this plot
        /// </summary>
        /// <param name="plot">The plot containing locations of its land</param>
        /// <returns>the bordering tile location that extends this plot</returns>
        public Vector2Int getRandAdjPlotLoc()
        {
            List<Vector2Int> eligibleTileLocs = getAdjPlotLocs();
            int randIdx = UnityEngine.Random.Range(0, eligibleTileLocs.Count);
            return eligibleTileLocs[randIdx];
        }

        /// <summary>
        /// Add a tile to this plot
        /// </summary>
        public void addEquipmentToPlot(Vector2Int plotLoc, Equipment equipment)
        {
            this.changeCashReserves(-equipment.price);
            this.addTile(plotLoc);
            Map.updateBoundingBox(plotLoc, boundingBox);
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
            boundingBox = createBoundingBox(buffer, id);
        }

        public List<Vector2Int> getTiles()
        {
            return this.tiles;
        }
    }

}