using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/**
 * A solar panel
 */
public class PVModule : Equipment
{
    public int wattHours { get; private set; }
    public int watts { get; private set; }

    public PVModule(float price, int wattHours, Plot plot) : base(price, plot, Type.PVModule)
    {
        this.wattHours = wattHours;
        this.watts = (int)(wattHours * (GameManager.instance.energyDataCollectPeriod / Constants.MIN_IN_HOUR));
        GameManager.instance.StartCoroutine(collectEnergy());
    }

    /// <summary>
    /// Collect sunlilght and update the plot's total self produced energy
    /// </summary>
    private IEnumerator collectEnergy()
    {
        yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);
        plot.changeSelfProducedEnergy(watts);
    }
}