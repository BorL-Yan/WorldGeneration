using System;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode {Local, Global}
    
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale, 
        int seed,
        int octaves, float persistence, float lacunarity,
        Vector2 offset, NormalizeMode normalizeMode)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random random = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        float maxPossibleHeight = 0f;
        float amplitude = 1;
        float frequency = 1;
        
        for (int i = 0; i < octaves; i++)
        {
            float offsetX = random.Next(-100000, 100000) + offset.x;
            float offsetY = random.Next(-100000, 100000) - offset.y;
            octaveOffsets[i] = new Vector2(offsetX, offsetY);
            maxPossibleHeight += amplitude;
            amplitude *= persistence;
        }
        
        if (scale <= 0) scale = 0.001f;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;
        

        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;
        
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;
                
                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x-halfHeight + octaveOffsets[i].x ) / scale * frequency;
                    float sampleY = (y-halfWidth + octaveOffsets[i].y) / scale * frequency;

                    float perlinValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;

                }

                if (noiseHeight > maxLocalNoiseHeight) maxLocalNoiseHeight = noiseHeight;
                else if (noiseHeight < minLocalNoiseHeight) minLocalNoiseHeight = noiseHeight;
                
                
                noiseMap[x, y] = noiseHeight;
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                switch (normalizeMode)
                {
                    case NormalizeMode.Local:
                    {
                        noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                        break;
                    }
                    case NormalizeMode.Global:
                    {
                        float normalizeHeight = (noiseMap[x, y] + 1) / (maxPossibleHeight * 1.4f);
                        noiseMap[x, y] = Math.Clamp( normalizeHeight, 0, int.MaxValue); 
                        break;
                    }
                }
            }
        }

        return noiseMap;
    }
}
