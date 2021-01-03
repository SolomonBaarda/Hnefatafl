using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static bool IsWithinBounds<T>(in Vector2Int v, in T[,] array)
    {
        return IsWithinBounds(v.x, v.y, array);
    }

    public static bool IsWithinBounds<T>(in int x, in int y, in T[,] array)
    {
        return x >= 0 && y >= 0 && x < array.GetLength(0) && y < array.GetLength(1);
    }



}
