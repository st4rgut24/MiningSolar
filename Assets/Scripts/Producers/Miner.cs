using System.Collections;
using UnityEngine;
using Scripts;

/**
 * A machine that turns electricity into profit by ordering blocks in the blockchain using computational power
 */
public class Miner : Equipment
{

    public float maxHashingPower { get; private set; }
    public float activeHashingPower { get; private set; } // the current hashing power of the miner

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
    /// <param name="sprite">image of miner</param>
    public Miner(float hashingPower, int energyUsage, float price, Plot plot, Sprite sprite) : base(price, plot, Type.Miner, sprite)
    {
        this.maxHashingPower = hashingPower;
        this.activeHashingPower = hashingPower;

        this.rewards = 0;
        this.energyUsage = energyUsage;
        miningEfficiency = energyUsage / hashingPower;
        watts = energyUsage * (int)(GameManager.instance.energyDataCollectPeriod / Constants.MIN_IN_HOUR);

        GameManager.instance.StartCoroutine(spendEnergy());
        //Debug.Log("Created miner with id " + instId);
    }

    /// <summary>
    /// Win the coinbase, collect earnings
    /// </summary>
    /// <param name="coinbase">amount of bitcoin as miner reward</param>
    public void collectCoinbase(float coinbase, float bitcoinPrice)
    {
        float dollarAmount = coinbase * bitcoinPrice;
        rewards += dollarAmount;
        plot.changeCashReserves(dollarAmount);
        plot.changeBitcoinProd(coinbase);
    }

    /// <summary>
    /// Spend energy mining bitcorns
    /// </summary>
    private IEnumerator spendEnergy()
    {
        yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);
        bool isEnoughEnergy = this.plot.changeEnergyReserves(-watts);
        // power down the miner if there isn't enough energy produced by this plot of land
        this.activeHashingPower = isEnoughEnergy ? maxHashingPower : 0;
    }
}
