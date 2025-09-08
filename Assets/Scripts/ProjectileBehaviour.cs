using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class ProjectileBehaviour : MonoBehaviour
{
    public float Speed = 20f;
    [SerializeField] float lifeTime = 4f;

    Rigidbody2D rb;
    Collider2D col;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.bodyType = RigidbodyType2D.Kinematic;     // no pushback
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        col.isTrigger = true;                        // use triggers for hits
    }

    void OnEnable() => Destroy(gameObject, lifeTime);

    // Call this right after Instantiate
    public void Launch(Vector2 dir)
    {
        rb.velocity = dir.normalized * Speed;       // <-- movement by vector, not rotation
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Don't die on the player (safety)
        if (other.CompareTag("Player")) return;

        // If you only want to kill on enemies, uncomment:
        // if (!other.CompareTag("Enemy")) return;

        Destroy(gameObject);
    }
}
