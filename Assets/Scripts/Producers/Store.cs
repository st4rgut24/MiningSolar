using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

public class Store : MonoBehaviour
{
    [SerializeField]
    public string defaultMiner = AntminerS9.id;

    [SerializeField]
    public string defaultModule = HiA375.id;

    //Sprites

    [SerializeField]
    private Sprite AntminerS9Sprite;

    [SerializeField]
    private Sprite HiA375Sprite;

    public static Store instance;

    /// <summary>
    /// A miner factory
    /// </summary>
    public Dictionary<string, Func<Plot, Sprite, Miner>> MinerStore = new Dictionary<string, Func<Plot, Sprite, Miner>>()
    {
        {AntminerS9.id, (Plot plot, Sprite sprite) => new AntminerS9(plot, sprite) }
    };

    /// <summary>
    /// A PVModule factory
    /// </summary>
    public Dictionary<string, Func<Plot, Sprite, PVModule>> PVModuleStore = new Dictionary<string, Func<Plot, Sprite, PVModule>>()
    {
        { HiA375.id, (Plot plot, Sprite sprite) => new HiA375(plot, sprite) }
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
    /// Get the sprite corresponding to a store item
    /// </summary>
    /// <returns>the sprite</returns>
    public Sprite getSpriteFromId(string id)
    {
        if (id == AntminerS9.id)
        {
            return AntminerS9Sprite;
        }
        else if (id == HiA375.id)
        {
            return HiA375Sprite;
        }
        else
        {
            throw new Exception("Not a valid sprite for id " + id);
        }
    }

    /// <summary>
    /// Buy an item
    /// </summary>
    /// <param name="id">id of the bought item, defaults if null</param>
    /// <param name="equipmentType">The type of bought item</param>
    /// <param name="player">Player buying the item</param>
    /// <param name="plot">The plot where the item lives</param>
    /// <param name="loc">Location of the equipment tile</param>
    public void BuyItem(Equipment.Type equipmentType, IPlayer player, Plot plot, Vector2Int loc, string id=null)
    {
        // if no location is specified then choose a location adjacent to an existing player tile
        Vector2Int plotLoc = loc.Equals(GameManager.instance.NullableLoc) ? plot.getRandAdjPlotLoc() : loc;
        bool validTile = GameManager.instance.isValidTileLoc(plotLoc);
        if (!validTile)
        {
            return;
        }

        Equipment equipment = null;
        PlayerTile tile = null;
        string equipmentId = null;

        if (equipmentType == Equipment.Type.Miner)
        {
            equipmentId = id ?? Store.instance.defaultMiner;
            equipment = Store.instance.getMiner(equipmentId, plot);
            plot.addMiner((Miner) equipment);
        }
        else if (equipmentType == Equipment.Type.PVModule)
        {
            equipmentId = id ?? Store.instance.defaultModule;
            equipment = Store.instance.getPVModule(equipmentId, plot);
            plot.addModule((PVModule)equipment);
        }
        else
        {
            throw new System.Exception("Not a valid type " + equipmentType);
        }

        Sprite sprite = getSpriteFromId(equipmentId);
        tile = new PlayerTile(player, equipmentType, plotLoc, equipment, sprite);

        plot.addEquipmentToPlot(plotLoc, equipment);

        player.buyItem(equipment.instId);
        Map.addPlayerTile(tile);
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
        Sprite minerSprite = getSpriteFromId(minerId);
        return MinerStore.ContainsKey(minerId) ? MinerStore[minerId](plot, minerSprite) : null;
    }

    public PVModule getPVModule(string moduleId, Plot plot)
    {
        Sprite moduleSprite = getSpriteFromId(moduleId);
        return PVModuleStore.ContainsKey(moduleId) ? PVModuleStore[moduleId](plot, moduleSprite) : null;
    }
}
