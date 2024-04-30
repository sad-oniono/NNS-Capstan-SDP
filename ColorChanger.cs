using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ColorChanger : MonoBehaviour
{
    public static Color DetermineColor(double number, double max)
    {

        Color output;

        float red;
        float green;
        float ratio = (float)(number / max);
        red = (float)Math.Pow(Math.E, 0.693 * ratio) - 1;
        green = 2 - (float)Math.Pow(Math.E, 0.693 * ratio);
        output = new Vector4(red, green, 0, 1);

        return output;
    }
}
