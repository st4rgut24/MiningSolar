using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/**
 * A contract is a profit-sharing agreement between players. In the beginning, only contract will be
 * the exchange of energy for bitcoins
 */
public class Contract
{
    int contractDuration;

    const int MIN_CONTRACT_DURATION = 1;
    const int MAX_CONTRACT_DURATION = 6;

    public int wattsPerHour { get; private set; }

    float wattHourPrice;
    float energyCost; // the total cost of importing energy under contract terms = (watts_of_power * hours * price_per_watt_hour) 

    Plot otherPlot;
    Plot ownerPlot;

    bool isActive;
    bool ownerCancel;
    bool otherCancel;

    /// <summary>
    /// Contract contains terms of agreement
    /// </summary>
    /// <param name="wattsPerHour">The amount of energy in kWh being exported by contract owner</param>
    /// <param name="wattHourPrice">Price per watt that contract owner requests</param>
    /// <param name="duration">Duration of the contract measured in number of blocks</param>
    public Contract(int wattsPerHour, float wattHourPrice, int duration, Plot ownerPlot)
    {
        this.isActive = true;
        this.ownerCancel = false;
        this.otherCancel = false;

        this.wattsPerHour = wattsPerHour;
        this.wattHourPrice = wattHourPrice;

        this.contractDuration = duration;

        this.energyCost = wattsPerHour * this.contractDuration * this.wattHourPrice;

        this.ownerPlot = ownerPlot;
    }

    /// <summary>
    /// Randomly generate the duration of contract
    /// </summary>
    /// <returns>Number of blocks the contract will remain in effect</returns>
    public static int getContractDuration()
    {
        return Random.Range(MIN_CONTRACT_DURATION, MAX_CONTRACT_DURATION);
    }

    /// <summary>
    /// Add the other party's plot in the contract (besides the contract's creator)
    /// </summary>
    /// <param name="otherPlot">The plot belonging to the owner of the contract (the exporter)</param>
    public void addCounterparty(Plot otherPlot)
    {
        this.otherPlot = otherPlot;
    }

    /// <summary>
    /// Get the plot corresponding to the player id in the contract
    /// </summary>
    /// <param name="playerId"></param>
    /// <returns>a plot</returns>
    public Plot getPlot(string playerId)
    {
        return playerId == ownerPlot.getOwnerId() ? ownerPlot : otherPlot;
    }

    public bool getStatus()
    {
        return isActive;
    }

    /// <summary>
    /// Get the duration of the contract
    /// </summary>
    /// <returns></returns>
    public int getDuration()
    {
        return contractDuration;
    }

    /// <summary>
    /// Get the total cost of exported energy under contract
    /// </summary>
    /// <returns></returns>
    public float getEnergyCost()
    {
        return energyCost;
    }

    /// <summary>
    /// Both parties must agree to cancel a contract
    /// </summary>
    /// <param name="playerId">The id of the player requesting to cancel the contract</param>"
    public void cancelContract(string playerId)
    {
        if (playerId.Equals(ownerPlot.getOwnerId()))
        {
            this.ownerCancel = true;
        }
        if (playerId.Equals(otherPlot.getOwnerId()))
        {
            this.otherCancel = true;
        }
        if (this.ownerCancel && this.otherCancel)
        {
            this.isActive = false;
        }
    }
}
