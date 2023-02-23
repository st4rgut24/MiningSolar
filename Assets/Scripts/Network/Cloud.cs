using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

public class Cloud : MonoBehaviour
{
    public float updateInterval { get; private set; }
    public WeatherTile weatherTile { get; private set; }

    private Vector2Int[] Steps = new Vector2Int[4] { Vector2Int.right, Vector2Int.left, Vector2Int.up, Vector2Int.down};


    /// <summary>
    /// Start updating the weather
    /// </summary>
    /// <param name="weatherUpdateInterval">weather change frequency</param>
    /// <param name="wg"> weather generator class</param>
    public void init(WeatherGenerator wg,  float weatherUpdateInterval, WeatherTile weatherTile)
    {
        this.weatherTile= weatherTile;
        this.updateInterval = weatherUpdateInterval;
        StartCoroutine(Move(wg));
    }

    /// <summary>
    /// The cloud moves in a random step and notifies the weather manager
    /// </summary>
    /// <param name="wg">Weather generator class tracks weather patterns</param>
    public IEnumerator Move(WeatherGenerator wg)
    {
        while (true)
        {
            yield return new WaitForSeconds(this.updateInterval);
            int randStep = Random.Range(0, Steps.Length);
            Vector2Int deltaLoc = Steps[randStep];
            // wg has to bound this location within bounding box
            wg.updateWeatherPattern(deltaLoc, weatherTile);
        }
    }
}
