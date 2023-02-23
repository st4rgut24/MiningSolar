using Codice.Client.BaseCommands;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

namespace Scripts
{
    public class WeatherGenerator : MonoBehaviour
    {
        [SerializeField]
        Cloud cloud;

        [SerializeField]
        public Sprite cloudSprite {get; private set;}

        [SerializeField]
        public Sprite stormSprite { get; private set; }

        [SerializeField]
        public Sprite hellRainSprite { get; private set; }

        const float WEATHER_UPDATE_INTERVAL = 10; // seconds between weather pattern change

        /// <summary>
        /// Respond to changes in weather
        /// </summary>
        /// <param name="deltaLoc">Change in position of weather phenomenon</param>
        /// <param name="weatherTile">The tile containing the weather phenomenon before it moved</param>
        /// <returns>The updated weather tile</returns>
        public WeatherTile updateWeatherPattern(Vector2Int deltaLoc, WeatherTile weatherTile)
        {
            // decrement cloud count at old position of weather tile, removing tile if necessary
            updateWeatherTile(weatherTile, -1);

            Vector2Int updatedLoc = boundLoc(weatherTile.loc + deltaLoc);
            WeatherTile updatedWeatherTile = Map.getWeatherTile(updatedLoc);
            if (updatedWeatherTile != null)
            {
                return updateWeatherTile(updatedWeatherTile, 1);
            }
            else 
            {
                return Map.addWeatherTile(new WeatherTile(updatedLoc, cloudSprite));
            }
        }

        /// <summary>
        /// Get the sprite corresponding to the weather type
        /// </summary>
        /// <param name="weatherType">The type of weather</param>
        /// <returns>The sprite corresponding to the weather type</returns>
        private Sprite getWeatherSprite(WeatherTile.TileType weatherType)
        {
            switch (weatherType)
            {
                case WeatherTile.TileType.Cloud:
                    return cloudSprite;
                case WeatherTile.TileType.Storm:
                    return stormSprite;
                case WeatherTile.TileType.HellRain: 
                    return hellRainSprite;
                default: 
                    return null;
            }
        }

        /// <summary>
        /// Update the weather pattern based off of the new cloud count
        /// </summary>
        /// <param name="deltaCloudCount">Change in cloud count</param>
        /// <returns>The updated weather tile</returns>
        private WeatherTile updateWeatherTile(WeatherTile weatherTile, int deltaCloudCount)
        {
            weatherTile.cloudCount += deltaCloudCount;
            if (weatherTile.cloudCount == 0)
            {
                Map.deleteWeatherTile(weatherTile.loc);
            }
            else
            {
                weatherTile.updateCloudType();
                weatherTile.sprite = getWeatherSprite(weatherTile.type);
            }
            return weatherTile;
        }

        /// <summary>
        /// Wrap tile locations that don't fit within the map
        /// </summary>
        /// <param name="updatedLoc">Updated location of weather pattern</param>
        /// <returns>the bounded tile location</returns>
        private Vector2Int boundLoc(Vector2Int updatedLoc)
        {
            if (updatedLoc.x < Map.allPlotsBoundingBox.minX)
            {
                updatedLoc.x = Map.allPlotsBoundingBox.maxX;
            }
            if (updatedLoc.x > Map.allPlotsBoundingBox.maxX)
            {
                updatedLoc.x = Map.allPlotsBoundingBox.minX;
            }
            if (updatedLoc.y < Map.allPlotsBoundingBox.minY)
            {
                updatedLoc.y = Map.allPlotsBoundingBox.maxY;
            }
            if (updatedLoc.y > Map.allPlotsBoundingBox.maxY)
            {
                updatedLoc.y = Map.allPlotsBoundingBox.minY;
            }
            return updatedLoc;
        }

        /// <summary>
        /// Get tile placement of first weather tile
        /// </summary>
        /// <returns>First weather tile's location</returns>
        private Vector2Int getRandomStartLoc()
        {
            int randX = Random.Range(Map.allPlotsBoundingBox.minX, Map.allPlotsBoundingBox.maxX);
            int randY = Random.Range(Map.allPlotsBoundingBox.minY, Map.allPlotsBoundingBox.maxY);
            return new Vector2Int(randX, randY);
        }

        public void GetCloud()
        {
            Cloud newCloud = Instantiate(cloud);
            Vector2Int startLoc = getRandomStartLoc();
            WeatherTile weatherTile = new WeatherTile(startLoc, cloudSprite);
            Map.addWeatherTile(weatherTile);
            newCloud.init(this, WEATHER_UPDATE_INTERVAL, weatherTile);
        }
    }

}