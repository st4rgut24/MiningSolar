using Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject botPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    public int analysisTimeFrameLowerBound { get; private set; } = 5;

    [SerializeField]
    public int analysisTimeFrameUpperBound { get; private set; } = 10;

    [SerializeField]
    private float minCashRatio = 0.0f;

    [SerializeField]
    private float maxCashRatio = 1.0f;

    [SerializeField]
    private float minSelfSustainRatio = 0.0f;

    [SerializeField]
    private float maxSelfSustainRatio = 1.0f;

    public static PlayerManager instance;

    List<IPlayer> agents;

    public Player humanPlayer;

    public class BotProps
    {
        public float selfSustainRatio { get; private set; }
        public float cashRatio { get; private set; }
        public int analysisTimeFrame { get; private set; }

        public BotProps()
        {
            this.analysisTimeFrame = Random.Range(PlayerManager.instance.analysisTimeFrameLowerBound, PlayerManager.instance.analysisTimeFrameUpperBound);
            this.cashRatio = Random.Range(PlayerManager.instance.minCashRatio, PlayerManager.instance.maxCashRatio);
            this.selfSustainRatio = Random.Range(PlayerManager.instance.minSelfSustainRatio, PlayerManager.instance.maxSelfSustainRatio);
        }
    }

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
    /// Get the properties defining characteristics of a bot
    /// </summary>
    /// <returns>the economic prarameters of a bot</returns>
    public BotProps getBotProps()
    {
        return new BotProps();
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
        IPlayer player;
        //player = isBot ? new Bot() : new Player(); (for testing)
        GameObject playerGo = isBot ? Instantiate(botPrefab) : Instantiate(playerPrefab);
        player = playerGo.GetComponent<IPlayer>();
        if (!isBot)
        {
            humanPlayer = (Player)player;
        }
        agents.Add(player);
        Plot plot = PlotGenerator.instance.GetPlot(player); // a miner gets added to the starting tile
        player.initializePlot(plot);
    }
}
