using System;
using UnityEngine;

public static class FalloffGenerator
{
    public static float[,] GeneratorFalloffMap(int size)
    {
        float[,] map = new float[size, size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float simpleX = x / (float)size * 2 - 1;
                float simpleY = y / (float)size * 2 - 1;

                float value = Math.Max(Math.Abs(simpleX), Math.Abs(simpleY));
                map[x, y] = StepwiseInterpolationEvaluate(value);
            }
        }

        return map;
    }

    static float StepwiseInterpolationEvaluate(float value)
    {
        float a = 3;
        float b = 2.2f;
        
        // f (x) = x^a / ( x^a + (b - b*value) ^ a ) 

        return Mathf.Pow(value, a) / (Mathf.Pow(value, a) + Mathf.Pow(b - b * value, a));
    }
}
