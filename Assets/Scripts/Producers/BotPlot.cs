using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;
using System;

public class BotPlot : Plot
{
    Miner defaultMiner;

    public PlotReport curPlotReport { get; private set; }
    public PlotReport prevPlotReport { get; private set; }

    public Bot bot { get; private set; }

    public List<BotAction> botPlotActionList { get; private set; }

    float selfSustainRatio;
    float bitcoinProd;

    public BotPlot(Bot player, Vector2Int location, Plot prevAdjPlot) : base(player, location, prevAdjPlot)
    {
        this.bot = player;
        this.botPlotActionList = new List<BotAction>();
        this.selfSustainRatio = player.selfSustainRatio;

        defaultMiner = Store.instance.getDefaultMiner(this);
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
                EquipmentFactory.BuyItem(Equipment.Type.Miner, bot, this, GameManager.instance.NullableLoc);
                break;
            case BotAction.BuyPanel:
                EquipmentFactory.BuyItem(Equipment.Type.PVModule, bot, this, GameManager.instance.NullableLoc);
                break;
            case BotAction.CreateContract:
                Contract contract = createContract(plotReport);
                // broadcast contract to a player or bot
                break;
            case BotAction.Save:
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
        float avgWattCost = plotReport.getAvgWattCost();
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

        int prevEnergyProd = (prevPlotReport == null) ? 0 : prevPlotReport.cumEnergyProd;
        int prevTotalEnergy = (prevPlotReport == null) ? 0 : prevPlotReport.cumTotalEnergy;
        float prevBitcoinProd = (prevPlotReport == null) ? 0 : prevPlotReport.cumBitcoinProd;
        
        int totalEnergy = getTotalEnergyProduction();
        PlotReport.EnergyInfo energyStats = new PlotReport.EnergyInfo(selfProducedEnergy, prevEnergyProd, totalEnergy, prevTotalEnergy);
        
        PlotReport.BitcoinInfo bitcoinStats = new PlotReport.BitcoinInfo(bitcoinProd, prevBitcoinProd);
        
        curPlotReport = new PlotReport(cashReserves, energyStats, bitcoinStats, miners, modules, selfSustainRatio, bot.analysisTimeFrame);
        return curPlotReport;
    }
}
