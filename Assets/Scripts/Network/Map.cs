using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace Scripts
{
     /**
     * Stores all the tiles in the map
     */
    public static class Map
    {
        public static int MAP_WIDTH = 10;
        public static int MAP_HEIGHT = 5;

        public static BoundingBox allPlotsBoundingBox; // bounding box encompassing all player tiles

        static Dictionary<Vector2Int, PlayerTile> playerTileDict;
        static Dictionary<Vector2Int, WeatherTile> weatherTileDict;
        static Dictionary<string, HashSet<Vector2Int>> bufferedZoneDict; // <plot id, set of plot locations>

        static Map()
        {
            weatherTileDict = new Dictionary<Vector2Int, WeatherTile>();
            playerTileDict = new Dictionary<Vector2Int, PlayerTile>();
            allPlotsBoundingBox = new BoundingBox(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            bufferedZoneDict = new Dictionary<string, HashSet<Vector2Int>>();
        }

        /// <summary>
        /// When a new player tile is added, update the bounding box encompassing all plots if necessary
        /// </summary>
        /// <param name="location">location of the newly added plot</param>
        /// 
        // TODO: If there's only one player this will be a  1 by 1 bounding box which is too small
        private static void updateBoundingBox(Vector2Int location)
        {
            if (location.x < allPlotsBoundingBox.minX)
            {
                allPlotsBoundingBox.setMinX(location.x);
            }
            if (location.x > allPlotsBoundingBox.maxX)
            {
                allPlotsBoundingBox.setMaxX(location.x);
            }
            if (location.y < allPlotsBoundingBox.minY)
            {
                allPlotsBoundingBox.setMinY(location.y);
            }
            if (location.y > allPlotsBoundingBox.maxY)
            {
                allPlotsBoundingBox.setMaxY(location.y);
            }
            setMinBounds(allPlotsBoundingBox);
        }

        /// <summary>
        /// Update the set of tile locations that are off-limits to new plots
        /// </summary>
        /// <param name="boundingBox">A bounding box defining the buffer of an existing plot</param>
        public static void addBufferedTiles(BoundingBox boundingBox)
        {
            if (boundingBox.id != null)
            {
                List<Vector2Int> bufferedLocs = boundingBox.getBufferedLocs();
                if (!bufferedZoneDict.ContainsKey(boundingBox.id))
                {
                    bufferedZoneDict.Add(boundingBox.id, new HashSet<Vector2Int>());
                }
                HashSet<Vector2Int> bufferedZone = bufferedZoneDict[boundingBox.id];
                for (int i = 0; i < bufferedLocs.Count; i++)
                {
                    Debug.Log("add buffered loc " + bufferedLocs[i]);
                    bufferedZone.Add(bufferedLocs[i]);
                }
            }
        }

        /// <summary>
        /// Update vertices of box if the width/height is less than the minimum bounds
        /// </summary>
        /// <param name="boundingBox">The bounding box</param>
        private static void setMinBounds(BoundingBox boundingBox)
        {
            if (boundingBox.width < MAP_WIDTH)
            {
                boundingBox.increaseBoxWidth(MAP_WIDTH);
            }
            if (boundingBox.height < MAP_HEIGHT)
            {
                boundingBox.increaseBoxHeight(MAP_HEIGHT);
            }
        }

        public static void deleteWeatherTile(Vector2Int loc)
        {
            weatherTileDict[loc] = null;
        }

        public static WeatherTile getWeatherTile(Vector2Int loc)
        {
            return weatherTileDict.ContainsKey(loc) ? weatherTileDict[loc] : null;
        }

        public static WeatherTile addWeatherTile(WeatherTile tile)
        {
            weatherTileDict[tile.loc] = tile;
            updateBoundingBox(tile.loc);
            return tile;
        }

        /// <summary>
        /// Add a player tile to the map if its location is not blocked
        /// </summary>
        /// <param name="tile">the tile to place</param>
        /// <returns>true if player tile is added</returns>
        public static void addPlayerTile(PlayerTile tile)
        {
            playerTileDict[tile.loc] = tile;
            updateBoundingBox(tile.loc);
            // draw a land tile and the player sprite overlaid on it
            Draw.instance.drawLandTile(tile.loc);
            Draw.instance.drawTile(tile);
        }

        /// <summary>
        /// Is the cell occupied by a player tile
        /// </summary>
        /// <param name="loc">location fo the tile</param>
        /// <returns></returns>
        public static bool isTileOccupied(Vector2Int loc)
        {
            return playerTileDict.ContainsKey(loc);
        }

        /// <summary>
        /// Is this tile location off limits for a new player tile
        /// </summary>
        /// <param name="loc">location of tile</param>
        /// <returns>true if tile exists already</returns>
        public static bool isPlayerTileInBufferZone(Vector2Int loc, string id)
        {
            foreach (KeyValuePair<string, HashSet<Vector2Int>> bufferedZone in bufferedZoneDict)
            {
                if (id != bufferedZone.Key)
                {
                    if (bufferedZone.Value.Contains(loc))
                    {
                        Debug.Log("tile at " + loc + " is in buffered zone");
                        return true;
                    }
                }
            }
            Debug.Log("tile at " + loc + " is not in buffered zone");
            return false;
        }
    }

}
