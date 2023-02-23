using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/**
 * An abstract class for generating locations of tiles
 */
public abstract class Generator
{
    public abstract Vector2Int GenerateRandomTile();
}
