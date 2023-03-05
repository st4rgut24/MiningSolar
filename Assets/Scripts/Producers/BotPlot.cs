using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;
using System;

public class BotPlot : Plot
{
    Scheduler scheduler;

    Miner defaultMiner;

    public PlotReport curPlotReport { get; private set; }
    public PlotReport prevPlotReport { get; private set; }

    public Bot bot { get; private set; }

    public List<BotAction> botPlotActionList { get; private set; }

    float selfSustainRatio;

    public BotPlot(Bot player, Vector2Int location, Plot prevAdjPlot, int proximity) : base(player, location, prevAdjPlot, proximity)
    {
        this.bot = player;
        this.botPlotActionList = new List<BotAction>();
        this.selfSustainRatio = player.selfSustainRatio;

        Store.instance.BuyItem(Equipment.Type.Miner, player, this, location);

    }

    /// <summary>
    /// Trigger an action based off a plot report
    /// </summary>
    /// <param name="botAction">The action the bot will take</param>
    /// <param name="plotReport"> The generated plot report that defines parameters of certain actions</param>
    public void schedulePlotAction(BotAction botAction, PlotReport plotReport)
    {
        botPlotActionList.Add(botAction);
        switch (botAction)
        {
            case BotAction.BuyMiner:
                Store.instance.BuyItem(Equipment.Type.Miner, bot, this, GameManager.instance.NullableLoc);
                Debug.Log("Buy a bitcoin miner");
                break;
            case BotAction.BuyPanel:
                Store.instance.BuyItem(Equipment.Type.PVModule, bot, this, GameManager.instance.NullableLoc);
                Debug.Log("Buy a pv module");
                break;
            case BotAction.CreateContract:
                Contract contract = createContract(plotReport);
                // broadcast contract to a player or bot
                Debug.Log("broadcast contract to a player or bot");
                break;
            case BotAction.Save:
                Debug.Log("save income");
                break;
            default:
                break;

        }
        bot.scheduleAction(botAction);
    }

    /// <summary>
    /// Creates a contract based off the latest plot report
    /// </summary>
    /// <param name="plotReport"> A report with energy production details</param>
    private Contract createContract(PlotReport plotReport)
    {
        int contractDuration = Contract.getContractDuration();
        int surplusEnergy = plotReport.getSurplusEnergy();
        int wattsPerBlock = surplusEnergy / contractDuration;

        int activeWattProd = getEnergyProd();
        float avgWattCost = plotReport.getAvgWattCost(activeWattProd);

        float price = avgWattCost * wattsPerBlock * contractDuration;
        return new Contract(wattsPerBlock, price, contractDuration, this);
    }

    /// <summary>
    /// Report plot stats and update the current plot report
    /// </summary>
    /// <returns>Plot Report or null if there is no new plot report since last we checked</returns>
    public PlotReport updatePlotReport()
    {
        prevPlotReport = curPlotReport;

        int cumTotalEnergy = cumSelfProducedEnergy + cumImportedEnergy;
        PlotReport.EnergyInfo energyStats = new PlotReport.EnergyInfo(cumSelfProducedEnergy, cumImportedEnergy, cumTotalEnergy, prevPlotReport);
        
        PlotReport.BitcoinInfo bitcoinStats = new PlotReport.BitcoinInfo(cumBitcoinProduced, prevPlotReport);
        
        curPlotReport = new PlotReport(cashReserves, energyStats, bitcoinStats, miners, modules, bot.botProps);
        return curPlotReport;
    }
}
