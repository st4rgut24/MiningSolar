using System.Collections;
using System.Collections.Generic;
using Scripts;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class StoreCanvas : Singleton<StoreCanvas>
{
    [SerializeField]
    Grid grid;

    [SerializeField]
    Tilemap HighlightTilemap;

    [SerializeField]
    Button buyMinerBtn;

    [SerializeField]
    Button buyModuleBtn;

    Dictionary<Vector2Int, Plot> PlotDict; // <tile location, plot>

    string selectedEquipmentId;
    Equipment.Type equipmentType;

    protected override void Awake()
    {
        base.Awake();
        buyMinerBtn.onClick.AddListener(buyDefaultMiner);
        buyModuleBtn.onClick.AddListener(buyDefaultModule);

        PlotDict = new Dictionary<Vector2Int, Plot>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (selectedEquipmentId != null)
            {
                placeTile(Input.mousePosition);
            }
        }
    }

    /// <summary>
    /// Place a equipment tile at the touch location if the tile is eligible
    /// </summary>
    /// <param name="mousePos">the mouse Position</param>
    private void placeTile(Vector3 mousePos)
    {
        Vector3Int touchedTile = getTileFromTouch(mousePos);
        Debug.Log("touched tile is " + touchedTile);
        bool isEligible = HighlightTilemap.GetTile(new Vector3Int(0,1,0)) != null;
        Vector2Int tileLoc = touchedTile.toVector2Int();
        if (isEligible && PlotDict.ContainsKey(tileLoc))
        {
            Plot plot = PlotDict[tileLoc];
            Store.instance.BuyItem(equipmentType, PlayerManager.instance.humanPlayer, plot, tileLoc, selectedEquipmentId);
            List<Vector2Int> adjLocs = plot.getAdjPlotLocs();
            adjLocs.ForEach((loc) => HighlightTilemap.SetTile(loc.toVector3Int(), null));
        }
        selectedEquipmentId = null;
    }

    private Vector3Int getTileFromTouch(Vector3 position)
    {
        Vector3 worldPointPos = Camera.main.ScreenToWorldPoint(position);
        Debug.Log("world point pos is " + worldPointPos);
        Vector3Int selectedCellPos = HighlightTilemap.WorldToCell(worldPointPos);
        return selectedCellPos;
    }

    private void buyDefaultMiner()
    {
        selectedEquipmentId = Store.instance.defaultMiner;
        equipmentType = Equipment.Type.Miner;
        highlightBuyLocs();
    }

    private void buyDefaultModule()
    {
        selectedEquipmentId = Store.instance.defaultModule;
        equipmentType = Equipment.Type.PVModule;
        highlightBuyLocs();
    }

    /// <summary>
    /// highlight adjacent tiles where the user can place the purchased equipment
    /// </summary>
    private void highlightBuyLocs()
    {
        PlotDict.Clear();
        Player player = PlayerManager.instance.humanPlayer;
        player.plots.ForEach((plot) =>
        {
            List<Vector2Int> adjLocs = plot.getAdjPlotLocs();
            Draw.instance.highlightTiles(adjLocs);
            addPlotLocsToDict(adjLocs, plot);
        });
    }

    /// <summary>
    /// Save the plot locations to the dictionary for lookup when the user selects a tile
    /// </summary>
    /// <param name="adjLocs">tile locations adjacent to a plot eligible for new tiles</param>
    private void addPlotLocsToDict(List<Vector2Int>adjLocs, Plot plot)
    {
        adjLocs.ForEach((loc) =>
        {
            Debug.Log("assign loc " + loc + " to my plot");
            PlotDict[loc] = plot;
        });
    }
}
