using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/// <summary>
/// Each bot has a scheduler which dictates the actions taken on its plots
/// </summary>
public class Scheduler: MonoBehaviour
{
    [SerializeField]
    int offerDuration;

    public int analysisTimeFrame;

    BotPlot plot;

    float selfSustainRatio;
    float cashRatio;

    /// <summary>
    /// Schedules bot actions based on predetermined targets
    /// </summary>
    /// <param name="selfSustainRatio">The percentage of mining power on a farm that is self-generated using solar</param>
    /// <param name="analysisTimeFrame">The time frame in minutes over which the current production data is gathered 
    /// after which a decision is made. Analagous to short-term/long-term investing</param>
    /// <param name="cashRatio"/>The Ratio of cash to investments. A higher percent implies more caution investing. 
    /// This ratio may be dynamic based on market conditions. Eg during bearish conditions, a bullish investor would adopt a 
    /// lower cash ratio. </param>
    public void initialize(BotPlot plot, float selfSustainRatio, float cashRatio, int analysisTimeFrame)
    {
        this.analysisTimeFrame = analysisTimeFrame;
        this.selfSustainRatio = selfSustainRatio;
        this.cashRatio = cashRatio;
        this.plot = plot;

        StartCoroutine(ScheduleActions());
    }

    /// <summary>
    /// Start scheduling actions for a bot's plot at a predefined interval using passed-in plot parameters
    /// </summary>
    /// <returns></returns>
    public IEnumerator ScheduleActions()
    {
        while (true)
        {
            yield return new WaitForSeconds(this.analysisTimeFrame);
            PlotReport plotReport = plot.updatePlotReport();
            BotAction botAction = chooseBotAction(plotReport);
            this.plot.schedulePlotAction(botAction, plotReport);
        }
    }

    /// <summary>
    /// Based on plot statistics choose a bot action
    /// </summary>
    /// <param name="plotReport">A report on plot production</param>
    /// <returns>The bot's next action</returns>
    BotAction chooseBotAction(PlotReport plotReport)
    {
        bool isSelfSustain = plotReport.isSelfSustainable();

        if (shouldInvest(plotReport.cash, plotReport.equipmentValue))
        {
            BotAction buyAction = isSelfSustain ? BotAction.BuyMiner : BotAction.BuyPanel;
            Equipment equipment = BotAction.BuyMiner == buyAction ? Store.instance.getDefaultMiner(plot) : Store.instance.getDefaultPVModule(plot);
            bool isEnoughMoney = isSavingsSufficient(equipment, plotReport.cash);
            return isEnoughMoney ? buyAction : BotAction.Save;
        }
        else if (isSelfSustain) // already self sustainable so ship off excess electricity
        {
            return BotAction.CreateContract;
        }
        else // save so you can invest in equipment
        {
            return BotAction.Save;
        }
    }

    /// <summary>
    /// Does current cash cover cost of equipment
    /// </summary>
    /// <param name="equipment">equipment wed like to purchase</param>
    /// <param name="cash">cash reserves of plot</param>
    /// <returns>true if there's enough cash to buy the equipment</returns>
    private bool isSavingsSufficient(Equipment equipment, float cash)
    {
        return cash >= equipment.price;
    }

    /// <summary>
    /// Should the bot invest
    /// </summary>
    /// <returns>true if yes</returns>
    public bool shouldInvest(float cash, float equipmentValue)
    {
        float savingsRatio = cash / equipmentValue;
        return savingsRatio > cashRatio;
    }
}
