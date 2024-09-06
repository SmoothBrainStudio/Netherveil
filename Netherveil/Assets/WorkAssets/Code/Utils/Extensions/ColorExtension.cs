using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class ColorExtension
{
    public static Color Color(string hex)
    {
        string redStr = (hex.Substring(0, 2));
        string greenStr = (hex.Substring(2, 2));
        string blueStr = (hex.Substring(4, 2));

        float red = redStr.FromHexString() / 255f;
        float green = greenStr.FromHexString() / 255f;
        float blue = blueStr.FromHexString() / 255f;

        return new Color(red, green, blue);
    }

    public static Color Color(int r, int g, int b)
    {

        float red = r / 255f;
        float green = g / 255f;
        float blue = b / 255f;

        return new Color(red, green, blue);
    }

    private static int FromHexString(this string hexString)
    {
        int num = int.Parse(hexString, System.Globalization.NumberStyles.HexNumber);

        return num;
    }
}
