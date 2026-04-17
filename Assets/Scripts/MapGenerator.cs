using Unity.Collections;
using UnityEngine;
using UnityEngine.Serialization;


public class MapGenerator : MonoBehaviour
{
    public DrawMode drawMode;

    public MapDisplay mapDisplay;
    public bool autoUpdate;
    
    [ReadOnly] public const int mapChunkSize = 241;
    [Range(0,6)] public int levelOfDetail;
    [Min(0.0001f)] public float noiseScale;

    [Min(1)] public int octaves;
    [Range(0f,1f)] public float persistence;
    [Min(1f)] public float lacunarity;
    
    public int seed;
    public Vector2 Offset;

    [Min(0f)] public float meshHeightMultiplier;
    [BoundedCurve(0,-1f, 1f, 2f)] public AnimationCurve meshHeightCurve;
    
    public Gradient gradient;
    
    
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, noiseScale, 
            seed,
            octaves, persistence, lacunarity,
            Offset);
        
        
        switch (drawMode)
        {
            case DrawMode.NoiseMap:
            {
                var texture = TextureGenerator.TextureFromHeightMap(noiseMap);
                mapDisplay.DrawTexture(texture);
                break;
            }
            case DrawMode.ColorMap:
            {
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(noiseMap), mapChunkSize, mapChunkSize);
                mapDisplay.DrawTexture(texture);
                break;
            }
            case DrawMode.Mesh:
            {
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(noiseMap), mapChunkSize, mapChunkSize);
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve, levelOfDetail);
                mapDisplay.DrawMesh(meshData, texture);
                break;
            }
        }
    }

    private Color[] GetColorMapToGradient(float[,] noiseMap)
    {
        Color[] colorMap = new Color[mapChunkSize * mapChunkSize];
        for (int y = 0; y < mapChunkSize; y++)
        {
            for (int x = 0; x < mapChunkSize; x++)
            {
                float currentHeight = noiseMap[x, y];
                colorMap[y * mapChunkSize + x] = gradient.Evaluate(currentHeight);
            }
        }

        return colorMap;
    }
}




public enum DrawMode {NoiseMap, ColorMap, Mesh}

[System.Serializable]
public struct TerrainType
{
    public string name;
    public float height;
    public Color color;
}


