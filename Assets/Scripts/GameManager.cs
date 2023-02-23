using Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private int proximity; // how far away a plot should be placed from another's plot

    [SerializeField]
    public Vector2Int defaultLoc { get; private set; }

    [SerializeField]
    public int energyDataCollectPeriod { get; private set; } = 10; // minutes between collecting energy usage data

    public Vector2Int NullableLoc = new Vector2Int(Int32.MaxValue, Int32.MaxValue);

    public static GameManager instance;
    public EquipmentFactory equipmentFactory { get; private set; }

    void Start()
    {
        if (instance != null && instance != this) {
            Destroy(gameObject);
        }
        else
        {
            initialize();
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void initialize()
    {
        equipmentFactory = new EquipmentFactory();
    }

    /// <summary>
    /// Test if this tile location is valid or not
    /// </summary>
    /// <param name="loc">location of tile in game board</param>
    /// <returns>true if tile loc is valid</returns>
    public bool isValidTileLoc(Vector2Int loc)
    {
        return !loc.Equals(NullableLoc);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
