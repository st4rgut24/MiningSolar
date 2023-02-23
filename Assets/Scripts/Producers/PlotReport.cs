using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public int selfGenEnergyThisReport { get; private set; }
    public int totalEnergyThisReport { get; private set; }
    public float curBitcoinProd { get; private set; }
    public int targetSelfGenEnergy { get; private set; }
    public int cumTotalEnergy { get; private set; }
    public int cumEnergyProd { get; private set; }
    public float cumBitcoinProd { get; private set; }
    public bool selfSustainable { get; private set; }


    public class EnergyInfo
    {
        public int cumSelfProducedEnergy { get; private set; }
        public int prevSelfProducedEnergy { get; private set; }
        public int totalEnergy { get; private set; }
        public int prevTotalEnergy { get; private set; }

        /// <summary>
        /// Energy related info in the plot report
        /// </summary>
        /// <param name="cumSelfProducedEnergy">The energy produced by the plot so far</param>
        /// <param name="prevSelfProducedEnergy">The amount of energy produced in the last plot report</param>
        /// <param name="totalEnergy">The total amount of energy including imported energy</param>
        /// <param name="prevTotalEnergy">The total amount of energy produced up until the previous plot report</param>
        public EnergyInfo(int cumSelfProducedEnergy, int prevSelfProducedEnergy, int totalEnergy, int prevTotalEnergy) {
            this.cumSelfProducedEnergy = cumSelfProducedEnergy;
            this.prevSelfProducedEnergy= prevSelfProducedEnergy;
            this.totalEnergy = totalEnergy;
            this.prevTotalEnergy = prevTotalEnergy;
        }
    }

    public class BitcoinInfo
    {
        public float cumBitcoinProduced { get; private set; }
        public float prevBitcoinProduced { get; private set; }

        /// <summary>
        /// The plot's bitcoin production info
        /// </summary>
        /// <param name="cumBitcoinProduced">Amount of bitcoin produced by the plot so far</param>
        /// <param name="prevBitcoinProduced">Amount of bitcoin mined during the previous plot report period</param>
        public BitcoinInfo(float cumBitcoinProduced, float prevBitcoinProduced)
        {
            this.cumBitcoinProduced = cumBitcoinProduced;
            this.prevBitcoinProduced= prevBitcoinProduced;
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
    public PlotReport(float cash, EnergyInfo energyStats, BitcoinInfo bitcoinStats, List<Miner> miners, List<PVModule> modules, float selfSustainRatio, int analysisTimeFrame)
    {
        this.cash = cash;
        this.selfSustainRatio = selfSustainRatio;
        cumEnergyProd = energyStats.cumSelfProducedEnergy;
        cumBitcoinProd = bitcoinStats.cumBitcoinProduced;

        timestamp  = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        equipmentValue = getEquipmentValue(miners, modules);

        maxEnergy = getMinerCapacity(miners, analysisTimeFrame);
        selfGenEnergyThisReport = energyStats.cumSelfProducedEnergy - energyStats.prevSelfProducedEnergy;
        totalEnergyThisReport = energyStats.totalEnergy - energyStats.prevTotalEnergy;
        
        curBitcoinProd = bitcoinStats.cumBitcoinProduced - bitcoinStats.prevBitcoinProduced;
        targetSelfGenEnergy = (int)(maxEnergy * selfSustainRatio);
    }

    /// <summary>
    /// Get the surplus energy (energy in excesss of self sustainable energy quota)
    /// </summary>
    /// <returns>The amount of excess energy in watts</returns>
    public int getSurplusEnergy()
    {
        return isSelfSustainable() ? (selfGenEnergyThisReport - targetSelfGenEnergy) : 0;
    }

    /// <summary>
    /// Calculate the value of a watt in terms of bitcoin production during this report's period
    /// </summary>
    /// <returns>The average watt cost in usd</returns>
    public float getAvgWattCost()
    {
        return curBitcoinProd / totalEnergyThisReport * RewardGenerator.bitcoinExchangeRate;
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
        return targetSelfGenEnergy < selfGenEnergyThisReport;
    }
}
