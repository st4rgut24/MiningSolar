using Scripts;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EquipmentFactory: Singleton<EquipmentFactory>
{
    public Sprite getSpriteFromId(string id)
    {
        return null;
    }

    /// <summary>
    /// Buy an item
    /// </summary>
    /// <param name="equipmentType">The type of bought item</param>
    /// <param name="player">Player buying the item</param>
    /// <param name="plot">The plot where the item lives</param>
    /// <param name="loc">Location of the equipment tile</param>
    public static void BuyItem (Equipment.Type equipmentType, IPlayer player, Plot plot, Vector2Int loc)
    {
        Vector2Int plotLoc = loc.Equals(GameManager.instance.NullableLoc) ? PlotGenerator.instance.getAdjPlotLoc(plot) : loc;
        bool validTile = GameManager.instance.isValidTileLoc(plotLoc);
        if (!validTile)
        {
            return;
        }

        Equipment equipment;
        PlayerTile tile;

        if (equipmentType == Equipment.Type.Miner)
        {
            Miner miner =  Store.instance.getDefaultMiner(plot);
            plot.addMiner(miner);
            tile = new PlayerTile(player, PlayerTile.TileType.Miner, plotLoc, null);
            equipment = miner;
        }
        else if (equipmentType == Equipment.Type.PVModule)
        {
            PVModule pvModule = Store.instance.getDefaultPVModule(plot);
            plot.addModule(pvModule);
            tile = new PlayerTile(player, PlayerTile.TileType.Solar, plotLoc, null);
            equipment = pvModule;
        }
        else
        {
            throw new System.Exception("Not a valid type " + equipmentType);
        }

        player.buyItem(equipment.instId);
        plot.changeCashReserves(equipment.price);
        Map.addPlayerTile(tile);
    }
}
