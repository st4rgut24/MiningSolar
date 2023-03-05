using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Scripts;
using UnityEngine;
using UnityEngine.TestTools;

public class BotTest
{
    Bot bot;
    Plot botPlot;
    Contract contract;
    PlayerManager playerManager;
    PlotGenerator pg;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var rewardGenObj = new GameObject();
        rewardGenObj.AddComponent<RewardGenerator>();

        var gameManagerObj = new GameObject();
        gameManagerObj.AddComponent<GameManager>();

        var playerManagerObj = new GameObject();
        playerManager = playerManagerObj.AddComponent<PlayerManager>();

        var storeObj = new GameObject();
        storeObj.AddComponent<Store>();

        var botObj = new GameObject();
        bot = botObj.AddComponent<Bot>();

        var plotGenObj = new GameObject();
        pg = plotGenObj.AddComponent<PlotGenerator>();

        yield return new WaitForEndOfFrame();
    }

    /// <summary>
    /// If the bot has no miners, reject the contract
    /// </summary>
    [Test]
    public void RejectContractIfNoMiners()
    {
        botPlot = pg.GetPlot(bot);
        contract = new Contract(100, 1, 10, botPlot);
        bool isAccepted = bot.negotiateContract(contract);
        Assert.IsFalse(isAccepted);
    }

    /// <summary>
    /// If the bot has enough energy to power its farm then it rejects the energy contract
    /// </summary>
    [Test]
    public void RejectContractDueToEnergySurplus()
    {
        botPlot = pg.GetPlot(bot);
        botPlot.addMiner(Store.instance.getMiner(AntminerS9.id, botPlot));
        int upperBoundMinerCapacity = PlotReport.getMinerCapacity(botPlot.miners, playerManager.analysisTimeFrameUpperBound);
        botPlot.changeEnergyReserves(upperBoundMinerCapacity + 1); 
        contract = new Contract(100, 1, 10, botPlot);
        bool isAccepted = bot.negotiateContract(contract);
        Assert.IsFalse(isAccepted);
    }

    /// <summary>
    /// Accept a contract if it is profitable for the bot
    /// </summary>
    [Test]
    public void AcceptProfitableContract()
    {
        botPlot = pg.GetPlot(bot);
        botPlot.addMiner(Store.instance.getMiner(AntminerS9.id, botPlot));
        int pricePerWattHour = 0;
        contract = new Contract(100, pricePerWattHour, 10, botPlot);
        bool isAccepted = bot.negotiateContract(contract);
        Assert.IsTrue(isAccepted);
    }

    /// <summary>
    /// Reject a contract if it is unprofitable for the bot
    /// </summary>
    [Test]
    public void RejectUnprofitableContract()
    {
        botPlot = pg.GetPlot(bot);
        botPlot.addMiner(Store.instance.getMiner(AntminerS9.id, botPlot));
        int contractDurationInBlocks = 10;
        float maxExpectedRewardPerBlock = RewardGenerator.REWARD_SIZE * RewardGenerator.BITCOIN_PRICE * contractDurationInBlocks;
        float pricePerWattHour = maxExpectedRewardPerBlock + 1;
        contract = new Contract(1, pricePerWattHour, 10, botPlot);
        bool isAccepted = bot.negotiateContract(contract);
        Assert.IsFalse(isAccepted);
    }
}
