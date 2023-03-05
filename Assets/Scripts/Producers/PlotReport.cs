using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/// <summary>
/// A report generated for a plot on its earnings and production metrics
/// </summary>
public class PlotReport
{
    public Int32 timestamp;
    public float cash { get; private set; }

    private float selfSustainRatio;

    public float equipmentValue { get; private set; }
    public int maxEnergy { get; private set; }

    public int totalEnergyThisReport { get; private set; }
    public int importedEnergyThisReport { get; private set; }

    public float bitcoinProdThisReport { get; private set; }

    public int targetSelfGenEnergy { get; private set; }
    public bool selfSustainable { get; private set; }

    public EnergyInfo energyInfo { get; private set; }
    public BitcoinInfo bitcoinInfo { get; private set; }

    /// <summary>
    /// Energy stats for a SINGLE report period
    /// </summary>
    public class EnergyInfo
    {
        public int cumSelfProducedEnergy { get; private set; }
        public int cumImportedEnergy { get; private set; }
        public int cumTotalEnergy { get; private set; }

        public int SelfProducedEnergy { get; private set; }
        public int ImportedEnergy { get; private set; }
        public int TotalEnergy { get; private set; }

        /// <summary>
        /// Energy related info in the plot report
        /// </summary>
        public EnergyInfo(int cumSelfProducedEnergy, int cumImportedEnergy, int cumTotalEnergy, PlotReport prevPlotReport) {

            // find the delta between the last plot report and this plot report's energy stats
            this.SelfProducedEnergy = cumSelfProducedEnergy - prevPlotReport?.energyInfo.SelfProducedEnergy ?? 0;
            this.ImportedEnergy = cumImportedEnergy - prevPlotReport?.energyInfo.ImportedEnergy ?? 0;
            this.TotalEnergy = cumTotalEnergy - prevPlotReport?.energyInfo.TotalEnergy ?? 0;

            this.cumSelfProducedEnergy = cumSelfProducedEnergy;
            this.cumImportedEnergy = ImportedEnergy;
            this.cumTotalEnergy = cumTotalEnergy;
        }
    }

    public class BitcoinInfo
    {
        public float BitcoinProducedThisReport { get; private set; }
        public float cumBitcoinProduced { get; private set; }

        /// <summary>
        /// The plot's bitcoin production info
        /// </summary>
        /// <param name="cumBitcoinProduced">Amount of bitcoin produced by the plot so far</param>
        /// <param name="prevBitcoinProduced">Amount of bitcoin mined during the previous plot report period</param>
        public BitcoinInfo(float cumBitcoinProduced, PlotReport prevPlotReport)
        {
            this.BitcoinProducedThisReport = cumBitcoinProduced - prevPlotReport?.bitcoinInfo.cumBitcoinProduced ?? 0;
            this.cumBitcoinProduced = cumBitcoinProduced;
        }
    }

    /// <summary>
    /// Report on a plot with pv modules and miners
    /// </summary>
    /// <param name="cash">The cash reserves of the plot</param>
    /// <param name="miners">mining machines on a plot</param>
    /// <param name="modules">modules on a plot</param>
    /// <param name="selfSustainRatio">self sustainable ratio of the plot</param>
    /// <param name="analysisTimeFrame">time frame over which analysis was made</param>
    public PlotReport(float cash, EnergyInfo energyStats, BitcoinInfo bitcoinStats, List<Miner> miners, List<PVModule> modules, PlayerManager.BotProps botProps)
    {
        this.energyInfo = energyStats;
        this.bitcoinInfo = bitcoinStats;

        this.cash = cash;
        this.selfSustainRatio = botProps.selfSustainRatio;

        timestamp  = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        equipmentValue = getEquipmentValue(miners, modules);

        maxEnergy = getMinerCapacity(miners, botProps.analysisTimeFrame);
        
        targetSelfGenEnergy = (int)(maxEnergy * selfSustainRatio);
    }

    /// <summary>
    /// Get the surplus energy (energy in excesss of self sustainable energy quota)
    /// </summary>
    /// <returns>The amount of excess energy in watts</returns>
    public int getSurplusEnergy()
    {
        return isSelfSustainable() ? (this.energyInfo.SelfProducedEnergy - targetSelfGenEnergy) : 0;
    }

    /// <summary>
    /// Calculate the value of a watt in terms of bitcoin production during this report's period
    /// </summary>
    /// <param name="botActiveWatts">Amount of watts the bot's plot is producing currently</param>
    /// <returns>The average watt cost in usd</returns>
    public float getAvgWattCost(int botActiveWatts)
    {
        if (botActiveWatts == 0)
        {
            throw new Exception("Attempting to calculate avg watt cost but the amount of watts is 0");
        }
        float totalEnergyProd = getTotalEnergyProduction();
        // proportion of total active wattage this player's plot owns
        float energyPct = (float)botActiveWatts / totalEnergyProd;
        float dollarReward = RewardGenerator.instance.calculateReward(energyPct);
        return dollarReward / botActiveWatts;
    }

    /// <summary>
    /// Get the total energy production across all plots
    /// </summary>
    /// <returns>watt hours of production</returns>
    public float getTotalEnergyProduction()
    {
        float totalActiveWatts = 0;
        PlotGenerator.instance.plots.ForEach((plot) =>
        {
            totalActiveWatts += plot.getEnergyProd();
        });
        return totalActiveWatts;
    }

    /// <summary>
    /// Get the maximum energy usage of the miners for a duration
    /// </summary>
    /// <param name="miners">mining equipment</param>
    /// <param name="durationMins">duration over which we are calculating</param>
    /// <returns>total watt hours used by all the miners</returns>
    public static int getMinerCapacity(List<Miner> miners, int durationMins)
    {
        int totalEnergyUsage = 0;

        for (int i = 0; i < miners.Count; i++)
        {
            Miner miner = miners[i];
            int hours = (int) (durationMins / Constants.MIN_IN_HOUR);
            hours = Math.Max(hours, 1);
            totalEnergyUsage += miner.energyUsage * hours;
        }
        return totalEnergyUsage;
    }

    /// <summary>
    /// Determine the value of all equipment including miners and pv modules
    /// </summary>
    /// <param name="miners">Miners</param>
    /// <param name="modules">PV Modules</param>
    /// <returns>The total dollar value of the equipment</returns>
    public static float getEquipmentValue(List<Miner> miners, List<PVModule> modules)
    {
        float totalPrice = 0;
        for (int i = 0; i < miners.Count; i++)
        {
            Miner miner = miners[i];
            totalPrice += miner.price;
        }
        for (int i = 0; i < modules.Count; i++)
        {
            PVModule module = modules[i];
            totalPrice += module.price;
        }
        return totalPrice;
    }

    /// <summary>
    /// Does the bot plot meet its quota (selfSustainRatio) for self sustainable energy production
    /// </summary>
    /// <returns>true if bot should invest in miners, false if bot should buy panels instead</returns>
    public bool isSelfSustainable()
    {
        Debug.Log("is Self Sustainable if self generated energy " + this.energyInfo.SelfProducedEnergy + " is greater than target energy generation " + targetSelfGenEnergy);
        return targetSelfGenEnergy < this.energyInfo.SelfProducedEnergy;
    }
}
