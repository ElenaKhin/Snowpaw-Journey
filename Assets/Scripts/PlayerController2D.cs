using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;              // optional
    public Transform groundCheck;          // optional (for gizmo only)
    public LayerMask groundLayer;          // ONLY real ground layers
    public Transform firePoint;            // optional, muzzle tip

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float groundCheckRadius = 0.15f; // gizmo only
    public int maxAirJumps = 0;            // set 1 for double jump

    [Header("Shooting")]
    public ProjectileBehaviour ProjectilePrefab;  // prefab with Launch(Vector2)
    public Transform LaunchOffset;                // preferred muzzle transform

    [Header("Grounding (robust)")]
    public float groundProbeDistance = 0.08f;
    public Vector2 groundProbeInset = new Vector2(0.05f, 0.10f);

    [Header("Jump feel (optional)")]
    public float coyoteTime = 0.10f;
    public float jumpBufferTime = 0.10f;

    [Header("Respawn")]
    public Transform respawnPoint;         // drop a checkpoint Transform here
    public float fallYThreshold = -10f;    // auto-respawn if player falls below this Y
    public float respawnInvulnTime = 0.2f; // disable collider briefly after respawn

    Rigidbody2D rb;
    Collider2D col;

    int airJumpsUsed = 0;
    float xInput;
    bool jumpPressedEdge;
    bool facingRight = true;

    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;
    bool respawning = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        rb.freezeRotation = true;
        if (!animator) animator = GetComponentInChildren<Animator>();

        if (rb.bodyType != RigidbodyType2D.Dynamic) rb.bodyType = RigidbodyType2D.Dynamic;
        if (rb.gravityScale <= 0f) rb.gravityScale = 3f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // movement input
        xInput = Input.GetAxisRaw("Horizontal");

        // jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedEdge = true;
            jumpBufferTimer = jumpBufferTime;
        }

        // shoot once per click
        if (Input.GetButtonDown("Fire1"))
            Shoot();

        // face movement direction
        if (xInput > 0 && !facingRight) Flip();
        else if (xInput < 0 && facingRight) Flip();

        // timers
        if (IsGrounded())
        {
            coyoteTimer = coyoteTime;
            airJumpsUsed = 0;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // auto-respawn on fall
        if (!respawning && transform.position.y < fallYThreshold)
            DoRespawn();

        // animator (guarded)
        if (animator)
        {
            if (HasParam("Speed", AnimatorControllerParameterType.Float))
                animator.SetFloat("Speed", Mathf.Abs(xInput));

            if (HasParam("Grounded", AnimatorControllerParameterType.Bool))
                animator.SetBool("Grounded", IsGrounded());

            if (HasParam("YVel", AnimatorControllerParameterType.Float))
                animator.SetFloat("YVel", rb.velocity.y);
        }
    }

    void FixedUpdate()
    {
        // run
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);

        // jump resolve
        if (jumpPressedEdge || jumpBufferTimer > 0f)
        {
            if (coyoteTimer > 0f)
            {
                DoJump();
            }
            else if (airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;
                DoJump();
            }

            jumpPressedEdge = false;
            jumpBufferTimer = 0f;
        }
    }

    void DoJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        coyoteTimer = 0f;

        if (animator && HasParam("Jump", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Jump");
    }

    bool IsGrounded()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(
            Mathf.Max(0.01f, b.size.x - groundProbeInset.x),
            Mathf.Max(0.01f, b.size.y - groundProbeInset.y)
        );
        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, groundProbeDistance, groundLayer);
        return hit.collider != null;
    }

    void Shoot()
    {
        if (ProjectilePrefab == null) return;

        Transform spawn = LaunchOffset ? LaunchOffset : (firePoint ? firePoint : transform);
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;

        const float forwardPad = 0.35f;
        const float upPad = 0.02f;
        Vector2 spawnPos = (Vector2)spawn.position + dir * forwardPad + Vector2.up * upPad;

        var proj = Instantiate(ProjectilePrefab, spawnPos, Quaternion.identity);

        var projCol = proj.GetComponent<Collider2D>();
        if (projCol)
        {
            foreach (var myCol in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(projCol, myCol, true);
        }

        proj.Launch(dir);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    // === Respawn handling ===
    public void DoRespawn()
    {
        StartCoroutine(CoRespawn());
    }

    IEnumerator CoRespawn()
    {
        respawning = true;

        // pick target point
        Vector3 target = respawnPoint ? respawnPoint.position
                                      : new Vector3(transform.position.x, 0f, transform.position.z);

        // reset motion/state
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        airJumpsUsed = 0;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        // teleport
        transform.position = target;

        // brief grace (avoid instant re-hit)
        if (respawnInvulnTime > 0f && col)
        {
            col.enabled = false;
            yield return new WaitForSeconds(respawnInvulnTime);
            col.enabled = true;
        }

        respawning = false;

        // optional: trigger animator
        if (animator && HasParam("Respawn", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Respawn");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // optional kill zone support
        if (!respawning && other.CompareTag("KillZone"))
            DoRespawn();
    }

    // --- Animator helper (prevents missing-parameter warnings) ---
    bool HasParam(string name, AnimatorControllerParameterType type)
    {
        if (!animator) return false;
        foreach (var p in animator.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        var c = GetComponent<Collider2D>();
        if (c)
        {
            Gizmos.color = Color.cyan;
            Bounds b = c.bounds;
            Vector2 size = new Vector2(
                Mathf.Max(0.01f, b.size.x - groundProbeInset.x),
                Mathf.Max(0.01f, b.size.y - groundProbeInset.y)
            );
            Gizmos.DrawWireCube(b.center + Vector3.down * groundProbeDistance,
                                new Vector3(size.x, size.y, 0.01f));
        }

        // Draw fall line for convenience
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-999f, fallYThreshold, 0f), new Vector3(999f, fallYThreshold, 0f));
    }
}
