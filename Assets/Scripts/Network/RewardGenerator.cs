using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardGenerator : MonoBehaviour
{
    public static RewardGenerator instance;

    public float totalHashPower { get; private set; }

    List<Miner> miners;
    public const float REWARD_SIZE = 6.5f;

    public const float bitcoinExchangeRate = 23000; // exchange rate of bitcoin for dollars

    private float blockTime = 600; // 10 minutes

    const int MAX_GUESS = 100000000;
    const int MIN_GUESS = 0;

    float leastCommonHash; // the lowest hashing miner used as common denominator
                           // when generating hash count of more powerful miners

    // Start is called before the first frame update
    void Start()
    {
        totalHashPower= 0;
        leastCommonHash = int.MaxValue;
        miners = new List<Miner>();

        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public float getBlockTime()
    {
        return blockTime;
    }

    public void setBlockTime(float blockTime)
    {
        this.blockTime = blockTime;
    }

    /// <summary>
    /// Calculate expected profits of a miner
    /// </summary>
    /// <param name="hashPower">Hash power of the miner(s)</param>
    /// <param name="numBlocks">duration is the number of blocks</param>
    /// <returns>Expected mining profits over a period of time</returns>
    public float calculateProfits(float hashPower, float numBlocks)
    {
        float winningPct = hashPower / totalHashPower; // probability of creating the next block and getting reward
        return winningPct * REWARD_SIZE * numBlocks * bitcoinExchangeRate;
    }

    /// <summary>
    /// Add a miner to list and assign leastCommonHash if it is the lowest hashing miner
    /// </summary>
    /// <param name="miner">The new miner</param>
    public void addMiner(Miner miner) {
        totalHashPower += miner.hashingPower;
        if (miner.hashingPower < leastCommonHash)
        {
            leastCommonHash = miner.hashingPower;
        }
        miners.Add(miner);
    }

    /// <summary>
    /// Reward a miner proportionate to its share of hashing power
    /// </summary>
    public IEnumerator ChooseRandomWinner()
    {
        while (true)
        {
            yield return new WaitForSeconds(blockTime);
            int luckyNum = Random.Range(MIN_GUESS, MAX_GUESS);

            Miner closestMiner = null;
            int closestDifference = int.MaxValue;

            miners.ForEach(miner =>
            {
                int guessCount = Mathf.FloorToInt(miner.hashingPower / leastCommonHash);
                updateClosestGuess(miner, guessCount, ref closestDifference, ref closestMiner, luckyNum);
            });
            if (closestMiner != null)
            {
                closestMiner.collectCoinbase(REWARD_SIZE);
            }
        }
    }

    /// <summary>
    /// Determine whether the miner's guess(es) are closest to the lucky number
    /// </summary>
    /// <param name="miner">Miner attempting guess</param>
    /// <param name="guessCount">Number of guesses a miner can make</param>
    /// <param name="closestMiner">Miner with the closest guess so far</param>
    /// <param name="closestDiff">The closest guess so far</param>
    /// <param name="luckyNum">The lucky number miners are trying to guess</param>
    /// <returns>the closest difference between lucky number and miner guess</returns>
    private void updateClosestGuess(Miner miner, int guessCount, ref int closestDiff, ref Miner closestMiner, int luckyNum)
    {
        for (int i = 0; i < guessCount; i++)
        {
            int minerGuess = Random.Range(MIN_GUESS, MAX_GUESS);
            int guessDiff = Mathf.Abs(luckyNum - minerGuess);
            if (guessDiff < closestDiff)
            {
                closestDiff = guessDiff;
                closestMiner = miner;
            }
        }
    } 
}
