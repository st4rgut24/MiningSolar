using System.Collections;
using System.Collections.Generic;
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

        static Map()
        {
            weatherTileDict = new Dictionary<Vector2Int, WeatherTile>();
            playerTileDict = new Dictionary<Vector2Int, PlayerTile>();
            allPlotsBoundingBox = new BoundingBox(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
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
        /// <returns></returns>
        public static bool addPlayerTile(PlayerTile tile)
        {
            bool isEmptyTile = !isPlayerTileExist(tile.loc);
            if (isEmptyTile)
            {
                playerTileDict[tile.loc] = tile;
                updateBoundingBox(tile.loc);
            }
            return isEmptyTile;
        }

        /// <summary>
        /// Is there a player tile at a location
        /// </summary>
        /// <param name="loc">location of tile</param>
        /// <returns>true if tile exists already</returns>
        public static bool isPlayerTileExist(Vector2Int loc)
        {
            return playerTileDict.ContainsKey(loc);
        }
    }

}
