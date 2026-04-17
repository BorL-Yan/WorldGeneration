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

    [Min(0f)] public float meshHeightMultiplier;
    [BoundedCurve] public AnimationCurve meshHeightCurve;
    
    public Gradient gradient;
    
    
    public void GenerateMap()
    {
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, noiseScale, 
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
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(noiseMap), mapWidth, mapHeight);
                mapDisplay.DrawTexture(texture);
                break;
            }
            case DrawMode.Mesh:
            {
                var texture = TextureGenerator.TextureFromColorMap(GetColorMapToGradient(noiseMap), mapWidth, mapHeight);
                MeshData meshData = MeshGenerator.GenerateTerrainMesh(noiseMap, meshHeightMultiplier, meshHeightCurve);
                mapDisplay.DrawMesh(meshData, texture);
                break;
            }
        }
    }

    private Color[] GetColorMapToGradient(float[,] noiseMap)
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


