using System;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public DrawMode drawMode;

    public MapDisplay mapDisplay;
    public bool autoUpdate;
    
    [Min(1)] public int mapWidth;
    [Min(1)] public int mapHeight;
    [Min(0.0001f)] public float noiseScale;

    [Min(1)] public int octaves;
    [Range(0f,1f)] public float persistence;
    [Min(1f)] public float lacunarity;

    public int seed;
    public Vector2 Offset;

    public TerrainType[] terrainRegions;

    public Gradient gradient;
    
    
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, 
            seed,
            octaves, persistence, lacunarity,
            Offset);
        
        Texture2D texture = null;
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
            {
                texture = TextureGenerator.TextureFromHeightMap(noiseMap);
                break;
            }
            case DrawMode.ColorMap:
            {
                Color[] colorMap = new Color[mapWidth * mapHeight];
                for (int y = 0; y < mapHeight; y++)
                {
                    for (int x = 0; x < mapWidth; x++)
                    {
                        float currentHeight = noiseMap[x, y];
                        colorMap[y * mapWidth + x] = gradient.Evaluate(currentHeight);
                    }
                }
                texture = TextureGenerator.TextureFromColorMap(colorMap, mapWidth, mapHeight);
                break;
            }
        }
        mapDisplay.DrawTexture(texture);
    }
}

public enum DrawMode {NoiseMap, ColorMap}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}


