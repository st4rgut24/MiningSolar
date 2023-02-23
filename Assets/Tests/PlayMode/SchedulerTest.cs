using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Scripts;
using UnityEngine;
using UnityEngine.TestTools;

public class SchedulerTest
{
    Scheduler scheduler;

    const int PROXIMITY = 1;

    const float selfSustainRatio = .5f;
    const float cashRatio = .5f;
    const int analysisTimeFrame = 5;

    CoroutineRunner coroutineRunner;
    Vector2Int defaultLoc = new Vector2Int(0, 0);
    BotPlot botPlot;

    Bot bot;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        var playerManagerObj = new GameObject();
        var testHelper = new GameObject();
        var botGameObject = new GameObject();
        var storeGameObject = new GameObject();
        var gameManagerGameObject = new GameObject();
        var rewardGenObj = new GameObject();
        var equipmentFactoryGo = new GameObject();

        rewardGenObj.AddComponent<RewardGenerator>();
        playerManagerObj.AddComponent<PlayerManager>();
        botGameObject.AddComponent<Bot>();
        storeGameObject.AddComponent<Store>();
        gameManagerGameObject.AddComponent<GameManager>();
        equipmentFactoryGo.AddComponent<EquipmentFactory>();

        coroutineRunner = testHelper.AddComponent<CoroutineRunner>();

        // wait til end of frame so Store can be initialized
        yield return new WaitForEndOfFrame();
        bot = botGameObject.GetComponent<Bot>();

        var plotGenGo = new GameObject();
        PlotGenerator pg = plotGenGo.AddComponent<PlotGenerator>();
        pg.initialize(PROXIMITY, defaultLoc);

        Plot plot = pg.GetPlot(bot);
        botPlot = (BotPlot)plot;

        botPlot.addMiner(Store.instance.getMiner(AntminerS9.id, botPlot));
        botPlot.addModule(Store.instance.getPVModule(HiA375.id, botPlot)); ;

        var gameObject = new GameObject();
        scheduler = gameObject.AddComponent<Scheduler>();
    }

    /// <summary>
    /// The user has not earned anything, so only thing to do is save (instead of spend on equipment)
    /// </summary>
    [UnityTest]
    public IEnumerator SchedulesSaveAction()
    {
        scheduler.initialize(botPlot, selfSustainRatio, cashRatio, analysisTimeFrame);
        coroutineRunner.testScheduleActionsCoroutine(scheduler);
        yield return new WaitForSeconds(scheduler.analysisTimeFrame);
        Assert.AreEqual(1, botPlot.botPlotActionList?.Count);
        var firstAction = botPlot.botPlotActionList[0];
        Assert.AreEqual(BotAction.Save, firstAction);
    }

    /// <summary>
    /// Generate enough cash for the bot to spend on equipment
    /// </summary>
    private void triggerInvest()
    {
        float equipmentValue = PlotReport.getEquipmentValue(botPlot.miners, botPlot.modules);
        botPlot.changeCashReserves(equipmentValue);
    }

    /// <summary>
    /// Save money when invest decision is made but not enough cash on hand
    /// </summary>
    [UnityTest]
    public IEnumerator SchedulesSaveWhenCashLessEquipment()
    {
        float smallCashRatio = .001f; // lower cash ratio allows investment decisions with smaller cash on hand
        Miner miner = Store.instance.getDefaultMiner(botPlot);
        botPlot.changeCashReserves(miner.price / 10); // cash not enough to cover cost of the default miner
        scheduler.initialize(botPlot, selfSustainRatio, smallCashRatio, analysisTimeFrame);
        coroutineRunner.testScheduleActionsCoroutine(scheduler);

        yield return new WaitForSeconds(scheduler.analysisTimeFrame);
        var firstAction = botPlot.botPlotActionList[0];
        Assert.AreEqual(BotAction.Save, firstAction);
    }

    /// <summary>
    /// When cash reserve quota met, spend on panels if self sustainability quota not met
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator SchedulesBuyModuleAction()
    {

        triggerInvest();
        scheduler.initialize(botPlot, selfSustainRatio, cashRatio, analysisTimeFrame);
        coroutineRunner.testScheduleActionsCoroutine(scheduler);

        yield return new WaitForSeconds(scheduler.analysisTimeFrame);
        var firstAction = botPlot.botPlotActionList[0];
        Assert.AreEqual(BotAction.BuyPanel, firstAction);
    }

    /// <summary>
    /// When cash reserve quota met, spend on miners if self sustainability quota met
    /// </summary>
    /// <returns></returns>
    [UnityTest]
    public IEnumerator SchedulesBuyMinerAction()
    {
        triggerInvest();
        // generate some energy
        yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);

        // schedule a plot report
        scheduler.initialize(botPlot, selfSustainRatio, cashRatio, analysisTimeFrame);
        coroutineRunner.testScheduleActionsCoroutine(scheduler);
        yield return new WaitForSeconds(scheduler.analysisTimeFrame);

        // assert the first action
        var firstAction = botPlot.botPlotActionList[0];
        Assert.AreEqual(BotAction.BuyMiner, firstAction);
    }

    /// <summary>
    /// Create a contract if cash reserve quota is not met and the plot is self sustainable
    /// </summary>
    [UnityTest]
    public IEnumerator SchedulesCreateContractAction()
    {
        yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);
        scheduler.initialize(botPlot, selfSustainRatio, cashRatio, analysisTimeFrame);
        coroutineRunner.testScheduleActionsCoroutine(scheduler);
        yield return new WaitForSeconds(scheduler.analysisTimeFrame);
        var firstAction = botPlot.botPlotActionList[0];
        Assert.AreEqual(BotAction.CreateContract, firstAction);
    }
}
