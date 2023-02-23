using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

public class Store : MonoBehaviour
{
    [SerializeField]
    string defaultMiner = AntminerS9.id;

    [SerializeField]
    string defaultModule = HiA375.id;

    public static Store instance;

    /// <summary>
    /// A miner factory
    /// </summary>
    public Dictionary<string, Func<Plot, Miner>> MinerStore = new Dictionary<string, Func<Plot, Miner>>()
    {
        {AntminerS9.id, (Plot plot) => new AntminerS9(plot) }
    };

    /// <summary>
    /// A PVModule factory
    /// </summary>
    public Dictionary<string, Func<Plot, PVModule>> PVModuleStore = new Dictionary<string, Func<Plot, PVModule>>()
    {
        { HiA375.id, (Plot plot) => new HiA375(plot) }
    };

    // Start is called before the first frame update
    void Start()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Assign a default miner to a plot
    /// </summary>
    /// <param name="plot">The plot miner belongs to</param>
    public Miner getDefaultMiner(Plot plot)
    {
        return getMiner(defaultMiner, plot);
    }

    /// <summary>
    /// Assign a default pv module to a plot
    /// </summary>
    /// <param name="plot">The plot the PV module belongs to</param>
    /// <returns>The plot the pv module belongs to</returns>
    public PVModule getDefaultPVModule(Plot plot)
    {
        return getPVModule(defaultModule, plot);
    }

    public Miner getMiner(string minerId, Plot plot)
    {
        return MinerStore.ContainsKey(minerId) ? MinerStore[minerId](plot) : null;
    }

    public PVModule getPVModule(string moduleId, Plot plot)
    {
        return PVModuleStore.ContainsKey(moduleId) ? PVModuleStore[moduleId](plot) : null;
    }
}
