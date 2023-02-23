using Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    public int analysisTimeFrameLowerBound { get; private set; } = 5;

    [SerializeField]
    public int analysisTimeFrameUpperBound { get; private set; } = 10;

    public static PlayerManager instance;

    List<IPlayer> agents;

    private void Start()
    {
        agents = new List<IPlayer>();

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
    
    /// <summary>
    /// Get a time frame a bot will perform analysis of plot stats using the lower and upper bounds
    /// </summary>
    public int getRandAnalysisTimeFrame()
    {
        return Random.Range(analysisTimeFrameLowerBound, analysisTimeFrameUpperBound);
    }

    /// <summary>
    /// Get a random player or bot
    /// </summary>
    /// <returns></returns>
    private IPlayer getRandomAgent()
    {
        int randPlayerAgent = Random.Range(0, agents.Count);
        return agents[randPlayerAgent];
    }

    /// <summary>
    /// Broadcast a contract an agent
    /// </summary>
    /// <param name="contract">The contract</param>
    public void broadcastContract(Contract contract)
    {
        IPlayer agent = getRandomAgent();
        agent.negotiateContract(contract);
    }

    /// <summary>
    /// Creates a player
    /// </summary>
    /// <param name="isBot">true if creating a bot player</param>
    public void createPlayer(bool isBot)
    {
        IPlayer player = isBot ? new Bot() : new Player();
        agents.Add(player);
        Plot plot = PlotGenerator.instance.GetPlot(player);
        PlayerTile tile = new PlayerTile(player, PlayerTile.TileType.Empty, plot.startingTile);
        Map.addPlayerTile(tile);
    }
}
