using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorExtensions
{
    public static Vector2Int toVector2Int(this Vector3Int vector)
    {
        return new Vector2Int(vector.x, vector.y);
    }

    public static Vector3Int toVector3Int(this Vector2Int vector)
    {
        return new Vector3Int(vector.x, vector.y, 0);
    }
}
