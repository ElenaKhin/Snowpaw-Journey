using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPos;          // anchor (center) where this segment is based
    private float length;            // width of this sprite in world units

    public Camera cam;               // assign Main Camera, or auto below
    [Range(0f, 1f)]
    public float parallaxEffect = 0.6f;

    void Start()
    {
        startPos = transform.position.x;

        // measure width from renderer (accounts for scale)
        var sr = GetComponent<SpriteRenderer>();
        if (sr != null)
            length = sr.bounds.size.x;
        else
            length = 10f; // fallback

        if (cam == null)
            cam = Camera.main;
    }

    void FixedUpdate()
    {
        if (cam == null) return;

        float camX = cam.transform.position.x;

        // Classic parallax: move this layer proportional to camera X
        float distance = camX * parallaxEffect;
        transform.position = new Vector3(startPos + distance, transform.position.y, transform.position.z);

        // Compute relative movement used for wrapping. The key is comparing to startPos.
        float temp = camX * (1f - parallaxEffect);

        // When the camera has moved more than one sprite length past our anchor,
        // shift the anchor by a full length. Note the (temp - startPos).
        if (temp - startPos >  length)
        {
            startPos += length;
        }
        else if (temp - startPos < -length)
        {
            startPos -= length;
        }
    }
}
