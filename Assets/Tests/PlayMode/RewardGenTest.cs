using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System;
using Scripts;

public class RewardGenTest
{
    RewardGenerator rewardGenerator;
    CoroutineRunner coroutineRunner;

    Miner miner;

    const float hashingPower = 100;
    const int energyUsage = 100;
    const float price = 100;

    Player player;
    PlotGenerator pg;

    [SetUp]
    public void SetUp()
    {
        var EquipmentFactoryGo = new GameObject();
        EquipmentFactoryGo.AddComponent<EquipmentFactory>();

        var GameManagerGameObject = new GameObject();
        GameManagerGameObject.AddComponent<GameManager>();

        var gameObject = new GameObject();
        rewardGenerator = gameObject.AddComponent<RewardGenerator>();

        var testerHelper = new GameObject();
        coroutineRunner = testerHelper.AddComponent<CoroutineRunner>();

        var playerGameObject = new GameObject();
        player = playerGameObject.AddComponent<Player>();

        var plotGenGo = new GameObject();
        pg = plotGenGo.AddComponent<PlotGenerator>();
        pg.initialize(1, new Vector2Int(0, 0));
    }

    // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
    // `yield return null;` to skip a frame.
    [UnityTest]
    public IEnumerator RewardGenTestWithEnumeratorPasses()
    {
        Plot plot = pg.GetPlot(player);
        miner = new AntminerS9(plot);
        rewardGenerator.addMiner(miner);

        const float TEST_BLOCK_TIME = 3;
        rewardGenerator.setBlockTime(TEST_BLOCK_TIME);
        coroutineRunner.testRewardGeneratorCoroutine(rewardGenerator);

        yield return new WaitForSeconds(TEST_BLOCK_TIME);
        Assert.AreEqual(RewardGenerator.REWARD_SIZE, miner.rewards);
    }
}
