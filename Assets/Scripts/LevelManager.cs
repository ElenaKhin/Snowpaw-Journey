using UnityEngine;

public class LevelManager : MonoBehaviour
{
    [Header("Chunks")]
    public GameObject firstChunk;        // Initial chunk
    public GameObject[] chunkPrefabs;    // Other chunks to spawn randomly

    [Header("Settings")]
    public int initialChunks = 5;        // How many chunks to spawn at start
    public float firstChunkY = -5.6f;    // Y position for initial chunk
    public float otherChunksY = -5.6f;  // Y position for other chunks

    private float nextChunkX = 0f;       // Tracks where next chunk should spawn

    void Start()
    {
        // Spawn the first chunk
        SpawnChunk(firstChunk, firstChunkY);

        // Spawn remaining chunks
        for (int i = 1; i < initialChunks; i++)
        {
            int rand = Random.Range(0, chunkPrefabs.Length);
            SpawnChunk(chunkPrefabs[rand], otherChunksY);
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
            leftOffset = sr.bounds.size.x * 0.5f; // distance from center to left edge
        }

        // Position next chunk at EndPoint + leftOffset
        nextChunkX += (endPoint.position.x - chunk.transform.position.x) + leftOffset;
    }


    Transform FindEndPoint(GameObject chunk)
    {
        // Try direct child first
        Transform end = chunk.transform.Find("EndPoint");
        if (end != null)
        {
            return end;
        }

        // Search recursively in all children
        foreach (Transform t in chunk.GetComponentsInChildren<Transform>())
        {
            if (t.name == "EndPoint")
            {
                Debug.Log("EndPoint found in child: " + t.name + " at " + t.position);
                return t;
            }
        }
        return null;
    }

}
