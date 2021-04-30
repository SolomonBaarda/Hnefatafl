using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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


    // Original code:
    // https://stackoverflow.com/questions/923771/quickest-way-to-convert-a-base-10-number-to-any-base-in-net


    private static readonly char[] Base62Characters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".ToCharArray();
    private static readonly char[] Base4Characters = "0123".ToCharArray();

    private static readonly Dictionary<char, int> CharValuesBase4 = Base4Characters.Select((c, i) => new { Char = c, Index = i }).ToDictionary(c => c.Char, c => c.Index);

    public static string LongToBase4(long value)
    {
        long targetBase = Base4Characters.Length;
        // Determine exact number of characters to use.
        char[] buffer = new char[Math.Max(
                   (int)Math.Ceiling(Math.Log(value + 1, targetBase)), 1)];

        var i = buffer.Length;
        do
        {
            buffer[--i] = Base4Characters[value % targetBase];
            value /= targetBase;
        }
        while (value > 0);

        return new string(buffer, i, buffer.Length - i);
    }

    public static long Base4ToLong(string number)
    {
        char[] chrs = number.ToCharArray();
        int m = chrs.Length - 1;
        int n = Base4Characters.Length, x;
        long result = 0;
        for (int i = 0; i < chrs.Length; i++)
        {
            x = CharValuesBase4[chrs[i]];
            result += x * (long)Math.Pow(n, m--);
        }
        return result;
    }



}
