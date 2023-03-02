using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Scripts;

public class AntminerS9 : Miner
{
    public static new string id = "AntminerS9";

    public AntminerS9(Plot plot, Sprite sprite): base(100, 1500, 100, plot, sprite)
    {
    }
}
