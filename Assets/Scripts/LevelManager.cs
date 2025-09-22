using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Chunks")]
    public GameObject firstChunk;        // Initial chunk
    public GameObject[] chunkPrefabs;    // Middle chunks to spawn randomly
    public GameObject finalChunk;        // Final chunk (ending)

    [Header("Settings")]
    public int middleChunks = 6;         // How many random middle chunks
    public float firstChunkY = -6f;    // Y position for initial chunk
    public float otherChunksY = -6f;   // Y position for other chunks

    private float nextChunkX = 0f;       // Tracks where next chunk should spawn

    void Start()
    {
        // Spawn the first chunk
        SpawnChunk(firstChunk, firstChunkY);

        // Spawn middle random chunks
        for (int i = 0; i < middleChunks; i++)
        {
            int rand = Random.Range(0, chunkPrefabs.Length);
            SpawnChunk(chunkPrefabs[rand], otherChunksY);
        }

        // Spawn the final chunk
        if (finalChunk != null)
        {
            SpawnChunk(finalChunk, otherChunksY);
        }
    }

    void SpawnChunk(GameObject prefab, float chunkY)
    {
        // Spawn chunk at nextChunkX
        GameObject chunk = Instantiate(prefab, new Vector3(nextChunkX, chunkY, 0f), Quaternion.identity);

        // Find EndPoint
        Transform endPoint = FindEndPoint(chunk);
        if (endPoint == null)
        {
            Debug.LogError(prefab.name + " is missing an EndPoint!");
            return;
        }

        // Calculate chunk width from root center to left edge
        SpriteRenderer sr = chunk.GetComponentInChildren<SpriteRenderer>();
        float leftOffset = 0f;
        if (sr != null)
        {
            leftOffset = sr.bounds.size.x * 0.5f;
        }

        // Position next chunk at EndPoint + leftOffset
        nextChunkX += (endPoint.position.x - chunk.transform.position.x) + leftOffset;
    }

    Transform FindEndPoint(GameObject chunk)
    {
        Transform end = chunk.transform.Find("EndPoint");
        if (end != null) return end;

        foreach (Transform t in chunk.GetComponentsInChildren<Transform>())
        {
            if (t.name == "EndPoint") return t;
        }
        return null;
    }
}
