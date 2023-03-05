using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scripts;

public class Equipment
{
    public Sprite sprite { get; private set; }
    
    public string instId;
    public float price { get; private set; }

    public Type type { get; private set; }

    protected Plot plot;

    public enum Type
    {
        Miner,
        PVModule
    }

    public Equipment(float price, Plot plot, Type equipmentType, Sprite sprite)
    {
        instId = Guid.NewGuid().ToString();
        this.plot = plot;
        this.price = price;
        this.type = equipmentType;
        this.sprite = sprite;
    }
}
