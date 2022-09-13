using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utility
{
    public static float Epsilon = 0.00001f;
    public static Vector2 Vec3Dto2D(Vector3 v)
    {
        return new Vector2(v.x, v.y);
    }

    public static string FormatNumber(int num)
    {
        // Ensure number has 3 significant digits at most
        int i = (int)Mathf.Pow(10, (int)Mathf.Max(0, Mathf.Log10(num) - 2));
        num = num / i * i;

        if (num >= 1000000000)
            return (num / 1000000000D).ToString("0.##") + "B";
        if (num >= 1000000)
            return (num / 1000000D).ToString("0.##") + "M";
        if (num >= 1000)
            return (num / 1000D).ToString("0.##") + "K";

        return num.ToString("#,0");
    }

    public static string FormatNumber(float num)
    {
        return num.ToString("0.##");
    }

    public static string FormatNumberSmall(float num)
    {
        return num.ToString("0.###");
    }

    public static bool IsBetween(Vector2 a, Vector2 b, Vector2 c, float r = 0)
    {
        // Returns true if b is between a and c
        return (((a.x + r >= b.x && b.x >= c.x - r) || (a.x + r >= b.x && b.x >= c.x - r)) &&
                ((a.y + r >= b.y && b.y >= c.y - r) || (a.y + r >= b.y && b.y >= c.y - r)));
    }
}
