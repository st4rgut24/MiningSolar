using System.Collections;
using UnityEngine;
using Scripts;

/**
 * A machine that turns electricity into profit by ordering blocks in the blockchain using computational power
 */
public class Miner : Equipment
{

    public float hashingPower { get; private set; }
    public int energyUsage { get; private set; } // energy usage per hour
    public float miningEfficiency { get; private set; }
    public float rewards { get; private set; }  // rewards earned over miner lifetime denominated in bitcoin
    public int watts { get; private set; } // amount of energy used per data collection period

    /// <summary>
    /// Miner specs
    /// </summary>
    /// <param name="hashingPower">Terhashes per second</param>
    /// <param name="energyUsage">watts per hour</param>
    /// <param name="price"> price of miner</param>
    public Miner(float hashingPower, int energyUsage, float price, Plot plot) : base(price, plot, Type.Miner)
    {
        this.rewards = 0;
        this.hashingPower = hashingPower;
        this.energyUsage = energyUsage;
        miningEfficiency = energyUsage / hashingPower;
        watts = energyUsage * (int)(GameManager.instance.energyDataCollectPeriod / Constants.MIN_IN_HOUR);

        GameManager.instance.StartCoroutine(spendEnergy());
    }

    /// <summary>
    /// Win the coinbase, collect earnings
    /// </summary>
    /// <param name="coinbase">amount of bitcoin as miner reward</param>
    public void collectCoinbase(float coinbase)
    {
        rewards += coinbase;
        plot.changeCashReserves(coinbase);
    }

    /// <summary>
    /// Spend energy mining bitcorns
    /// </summary>
    private IEnumerator spendEnergy()
    {
        yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);
        this.plot.changeEnergyReserves(-watts);
    }
}
