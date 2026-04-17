using System.Collections.Generic;
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const int maxViewDst = 450;
    public Transform viewer;
    public Material mapMaterial;
    public static Vector2 viewerPosition = new Vector2();
    private static MapGenerator mapGenerator;
    private int chunkSize;
    private int chunksVisibleViewDistance;

    private Dictionary<Vector2, TerrainChunk> _terrainChunkDictionary = new();
    private List<TerrainChunk> terrainChunksVisibleLastUpdate = new();
    
    private void Start()
    {
        mapGenerator = FindAnyObjectByType<MapGenerator>();
        chunkSize = MapGenerator.mapChunkSize - 1;
        chunksVisibleViewDistance = Mathf.RoundToInt(maxViewDst / chunkSize);
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        foreach (var lastChunks in terrainChunksVisibleLastUpdate)
        {
            lastChunks.SetVisible(false);
        }
        terrainChunksVisibleLastUpdate.Clear();
        
        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for (int yOffset = -chunksVisibleViewDistance; yOffset <= chunksVisibleViewDistance; yOffset++)
        {
            for (int xOffset = -chunksVisibleViewDistance; xOffset <= chunksVisibleViewDistance; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);

                if (_terrainChunkDictionary.TryGetValue(viewedChunkCoord, out var terrainChunk))
                {
                    terrainChunk.UpdateVisibleChunk();
                    if(terrainChunk.IsVisible) terrainChunksVisibleLastUpdate.Add(terrainChunk);
                }
                else 
                {
                    _terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }    
        }
    }

    public class TerrainChunk
    {
        private GameObject meshObject;
        private Vector2 position;
        private Bounds bounds;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;

        public TerrainChunk(Vector2 coord, int size,Transform parent, Material material)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);
            
            meshObject = new GameObject($"Terrain Chunk {coord.x}, {coord.y}");

            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshRenderer.material = material;
            
            meshObject.transform.position = positionV3;
            meshObject.transform.SetParent(parent);
            SetVisible(false);
            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
            
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            var mesh =  meshData.CreateMesh();
            meshFilter.mesh = mesh;
            
            if (mesh != null) Debug.Log("Create");
            else Debug.Log("Empty");
        }
        
        public void UpdateVisibleChunk()
        {
            float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDstFromNearestEdge <= maxViewDst;
            SetVisible(visible);
        }

        public void SetVisible(bool value) => meshObject.SetActive(value);

        public bool IsVisible => meshObject.activeSelf;
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        private int lod;

        public LODMesh(int lod)
        {
            this.lod = lod;
        }
        
        public void RequestMesh(MapData mapdata)
        {
            hasRequestedMesh = true;
            
        }
    }
}

