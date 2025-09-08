using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] Transform target;                 // Drag your Cat/player here
    [SerializeField] string playerTag = "Player";      // Fallback if target not set

    [Header("Position")]
    [SerializeField] Vector2 offset = new Vector2(2f, 0f);
    [SerializeField] bool lockY = true;
    [SerializeField] float fixedY = 0f;                // auto-filled from camera Y if lockY

    [Header("Smoothing")]
    [SerializeField] float smoothTime = 0.15f;

    [Header("Level Clamp (optional)")]
    [Tooltip("SpriteRenderer that spans the WHOLE level width.")]
    [SerializeField] SpriteRenderer levelBounds;
    [SerializeField] bool useAutoClamp = true;
    [SerializeField] float minX = -Mathf.Infinity;     // used if auto-clamp disabled
    [SerializeField] float maxX =  Mathf.Infinity;

    Vector3 _vel;
    Camera _cam;
    float _halfWidth;

    void Awake()
    {
        _cam = Camera.main ? Camera.main : GetComponent<Camera>();
        if (_cam == null) Debug.LogError("FollowPlayer: No Camera found on this GameObject.");
        if (_cam) _halfWidth = _cam.orthographicSize * _cam.aspect;

        if (transform.position.z >= 0f)                 // keep camera behind scene
            transform.position = new Vector3(transform.position.x, transform.position.y, -10f);

        if (lockY) fixedY = transform.position.y;
    }

    void Start()
    {
        // If target not assigned, try to find by tag once at start
        if (!target && !string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindGameObjectWithTag(playerTag);
            if (go) target = go.transform;
        }

        RecomputeClamps();

        // Snap to player immediately so we start centered
        if (target)
            transform.position = DesiredPosition();
        else
            Debug.LogWarning("FollowPlayer: No target assigned/found; camera will not follow.");
    }

    void LateUpdate()
{
    if (!target) return;

    float desiredY = lockY ? fixedY : target.position.y + offset.y;
    float desiredX = target.position.x + offset.x;
    desiredX = Mathf.Clamp(desiredX, minX, maxX);

    Vector3 targetPos = new Vector3(desiredX, desiredY, transform.position.z);
    transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref _vel, smoothTime);
}


    Vector3 DesiredPosition()
    {
        float desiredX = target.position.x + offset.x;
        desiredX = Mathf.Clamp(desiredX, minX, maxX);

        float desiredY = lockY ? fixedY : target.position.y + offset.y;
        return new Vector3(desiredX, desiredY, transform.position.z);
    }

    public void RecomputeClamps()
    {
        if (useAutoClamp && levelBounds && _cam)
        {
            Bounds b = levelBounds.bounds;
            minX = b.min.x + _halfWidth;
            maxX = b.max.x - _halfWidth;

            // Handle too-narrow level or wrong bounds
            if (minX > maxX)
            {
                float center = (b.min.x + b.max.x) * 0.5f;
                minX = maxX = center;
                Debug.LogWarning("FollowPlayer: Level bounds narrower than camera view (or wrong sprite). Locking X to center.");
            }
        }
        // else keep manual minX/maxX
    }

    // For spawn/respawn
    public void SetTarget(Transform t, bool snap = true)
    {
        target = t;
        if (snap && target) transform.position = DesiredPosition();
    }
}
