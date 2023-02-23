using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Scripts;
using UnityEngine;
using UnityEngine.TestTools;

public class ContractTest
{
    const int MOCK_WATTS = 3000;
    const int duration = 1000;
    const float MOCK_PRICE_PER_WATT = .1f;

    Plot ownerPlot;
    Plot otherPlot;

    Player contractOwner;
    Player contractOther;

    Contract contract;

    [SetUp]
    public void SetUp()
    {
        var contractOwnerGameObject = new GameObject();
        var contractOtherGameObject = new GameObject();
        
        contractOwner = contractOwnerGameObject.AddComponent<Player>();
        contractOther = contractOtherGameObject.AddComponent<Player>();
         
        ownerPlot = new Plot(contractOwner, new Vector2Int(1, 1), null);
        otherPlot = new Plot(contractOther, new Vector2Int(5, 5), null);
        
        contract = new Contract(MOCK_WATTS, MOCK_PRICE_PER_WATT, duration, ownerPlot);
        contract.addCounterparty(otherPlot);
    }

    /// <summary>
    ///  Initializes the contract
    /// </summary>
    [Test]
    public void InitializeContract()
    {
        const float EXPECTED_ENERGY_COST = MOCK_WATTS * duration * MOCK_PRICE_PER_WATT;
        float energyCost = contract.getEnergyCost();
        Assert.AreEqual(energyCost, EXPECTED_ENERGY_COST);
    }

    /// <summary>
    /// Retrieves the correct plot associated with an id
    /// </summary>
    [Test]
    public void GetPlot()
    {
        string id = contractOwner.getId();
        Plot plot = contract.getPlot(id);
        Assert.AreEqual(id, plot.getOwnerId());
    }

    /// <summary>
    /// Cancels a contract when both parties agree to
    /// </summary>
    [UnityTest]
    public IEnumerator CancelContract()
    {
        yield return new WaitForEndOfFrame(); // wait for id to be assigned to Contract Owner
        contract.cancelContract(contractOwner.getId());
        Assert.AreEqual(true, contract.getStatus());
        contract.cancelContract(contractOther.getId());
        Assert.AreEqual(false, contract.getStatus());
    }
}
