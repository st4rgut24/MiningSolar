using Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public enum BotAction
{
    CreateContract,
    BuyMiner,
    BuyPanel,
    Save,
    None
}

public class Bot : IPlayer
{
    [SerializeField]
    GameObject schedulerPrefab;

    public PlayerManager.BotProps botProps { get; private set; }
    public int analysisTimeFrame { get; private set; } // period in mins bewteen analysis of plots
    public float selfSustainRatio { get; private set; }
    public float cashRatio { get; private set; }

    List<BotAction> botActionList;

    protected override void Awake()
    {
        base.Awake();
        botProps = PlayerManager.instance.getBotProps();
    }

    protected override void Start()
    {
        base.Start();
        botActionList = new List<BotAction>();
    }

    /// <summary>
    /// In addition to creating a plot bots schedule actions for the plto
    /// </summary>
    /// <param name="plot">The new plot</param>
    public override void initializePlot(Plot plot)
    {
        base.initializePlot(plot);
        GameObject schedulerGo = Instantiate(schedulerPrefab, transform);
        Scheduler scheduler = schedulerGo.GetComponent<Scheduler>();
        scheduler.initialize((BotPlot) plot, botProps.selfSustainRatio, botProps.cashRatio, botProps.analysisTimeFrame);

    }

    /// <summary>
    /// Get the most recent bot action
    /// </summary>
    /// <returns>the bot action</returns>
    public BotAction getLastBotAction()
    {
        return botActionList.Count == 0 ? BotAction.None : botActionList[botActionList.Count - 1];
    }

    /// <summary>
    /// Perform a scheduleed action and save it
    /// </summary>
    /// <param name="action">The bot's action</param>
    public void scheduleAction(BotAction action)
    {
        botActionList.Add(action);
    }

    /// <summary>
    /// When receiving a contract, bot performs cost-benefit analysis. 
    /// Receiver of energy must weight the cost of utility costs with opportunity cost of idle miners.
    /// </summary>
    /// <param name="contract">the contract under negotiation</param>
    /// <returns>true if contract is accepted, false if rejected</returns>
    public override bool negotiateContract(Contract contract)
    {
        Plot plot = contract.getPlot(id);
        List<Miner> plotMiners = plot.getMiners();
        if (plotMiners.Count == 0)
        {
            return false;
        }
        int minerCapacity = PlotReport.getMinerCapacity(plot.miners, analysisTimeFrame);
        int totalEnergyProd = plot.getTotalEnergyProduction();
        return totalEnergyProd < minerCapacity ? executeCostBenefitAnalysis(plotMiners, contract) : false;
    }

    /// <summary>
    /// Perform comparison between energy costs and opportunity cost
    /// </summary>
    /// <param name="plotMiners">miners in plot</param>
    /// <param name="contract">The contract and its terms</param>
    /// <returns>true if bot should accept the contract (if the benefits outweight the cost)</returns>
    bool executeCostBenefitAnalysis(List<Miner>plotMiners, Contract contract) 
    {
        Miner miner = plotMiners[0]; // representative miner, future improvement would be to use the average miner
        float theoreticalMinerCount = miner.energyUsage / contract.wattsPerHour;
        float minerCount = Mathf.Min(plotMiners.Count, theoreticalMinerCount);
        float importedHashPower = miner.hashingPower * minerCount;
        float futureRewards = RewardGenerator.instance.calculateProfits(importedHashPower, contract.getDuration());
        return futureRewards > contract.getEnergyCost();
    }
}
