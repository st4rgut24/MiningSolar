using Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class IPlayer: MonoBehaviour
{
    [SerializeField]
    protected RewardGenerator rewardGenerator;

    public abstract bool negotiateContract(Contract contract);

    public string id;

    protected List<Plot> plots;
    protected string name;

    protected virtual void Awake()
    {
        plots = new List<Plot>();
    }

    protected virtual void Start()
    {
        this.plots = new List<Plot>();
        this.id = Guid.NewGuid().ToString();
    }

    /// <summary>
    /// Give a plot to a player
    /// </summary>
    /// <param name="plot">A plot of land</param>
    public virtual void initializePlot(Plot plot)
    {
        plots.Add(plot);
        this.buyItem(Equipment.Type.Miner, plot, plot.startingTile);
        Vector2Int adjTileLoc = plot.getAdjPlotLoc();
        this.buyItem(Equipment.Type.PVModule, plot, adjTileLoc); // a solar gets added to an adjacent tile
    }

    /// <summary>
    /// Buy an item from the store
    /// </summary>
    public void buyItem(Equipment.Type equipmentType, Plot plot, Vector2Int loc, string id = null)
    {
        Store.instance.BuyItem(equipmentType, this, plot, loc, id);
    }

    public void init(string name)
    {
        this.name = name;
    }

    /// <summary>
    /// Return whether type of object is a bot
    /// </summary>
    /// <returns>Get type of object</returns>
    public bool isBot()
    {
        return this.GetType() == typeof(Bot);
    }

    public string getId()
    {
        return id;
    }

    public void createContract(Contract contract, Guid playerId)
    {

    }

    public void destroyContract(Guid contractId)
    {

    }

    /// <summary>
    /// deduct the cash reserves of the player (plot?)
    /// </summary>
    /// <param name="itemId"></param>
    public void buyItem(string itemId)
    {

    }

    public void buyPlot(Vector2Int location)
    {

    }

    public void useItem(Guid itemId, Guid plotId)
    {

    }
}
