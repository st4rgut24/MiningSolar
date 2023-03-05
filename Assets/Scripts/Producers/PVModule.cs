using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

/**
 * A solar panel
 */
public class PVModule : Equipment
{
    public int maxWattHours { get; private set; }
    public int activeWattHours { get; private set; }

    public PVModule(float price, int wattHours, Plot plot, Sprite sprite) : base(price, plot, Type.PVModule, sprite)
    {
        this.maxWattHours = wattHours;
        this.activeWattHours = wattHours;

        GameManager.instance.StartCoroutine(collectEnergy());
        //Debug.Log("Created pv module with id " + instId);
    }

    public void shadePanel()
    {
        this.activeWattHours = 0;
    }

    public void unshadePanel()
    {
        this.activeWattHours = maxWattHours;
    }

    /// <summary>
    /// Collect sunlilght and update the plot's total self produced energy
    /// </summary>
    private IEnumerator collectEnergy()
    {
        while (true)
        {
            yield return new WaitForSeconds(GameManager.instance.energyDataCollectPeriod);
            plot.changeEnergyReserves(activeWattHours);
            plot.addSelfGeneratedEnergy(activeWattHours);
            //Debug.Log("PVModule " + instId + " generated " + activeWattHours + " watts");
        }
    }
}