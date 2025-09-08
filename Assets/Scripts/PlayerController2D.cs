using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;              // optional
    public Transform groundCheck;          // empty child at feet
    public LayerMask groundLayer;          // set to Ground layer
    public Transform firePoint;            // optional, muzzle tip

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float groundCheckRadius = 0.15f;
    public int maxAirJumps = 0;            // set to 1 for double jump, etc.

    [Header("Shooting")]
    public ProjectileBehaviour ProjectilePrefab;  // prefab with ProjectileBehaviour (has Launch(Vector2))
    public Transform LaunchOffset;                // preferred muzzle transform

    Rigidbody2D rb;
    int airJumpsUsed = 0;
    float xInput;
    bool jumpPressed;
    bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
        if (!animator) animator = GetComponentInChildren<Animator>();
    }

    void Update()
    {
        // movement input
        xInput = Input.GetAxisRaw("Horizontal");

        // jump input (edge)
        if (Input.GetButtonDown("Jump")) jumpPressed = true;

        // shoot once per click
        if (Input.GetButtonDown("Fire1"))
            Shoot();

        // face movement direction
        if (xInput > 0 && !facingRight) Flip();
        else if (xInput < 0 && facingRight) Flip();

        // animator (optional)
        if (animator)
        {
            animator.SetFloat("Speed", Mathf.Abs(xInput));
            animator.SetBool("Grounded", IsGrounded());
            animator.SetFloat("YVel", rb.velocity.y);
        }
    }

    void FixedUpdate()
    {
        // run
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);

        // jump
        if (jumpPressed)
        {
            if (IsGrounded())
            {
                airJumpsUsed = 0;
                Jump();
            }
            else if (airJumpsUsed < maxAirJumps)
            {
                airJumpsUsed++;
                Jump();
            }
        }
        jumpPressed = false;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, 0f); // consistent height
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        if (animator) animator.SetTrigger("Jump");
    }

    bool IsGrounded()
    {
        if (!groundCheck) return false;
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void Shoot()
    {
        if (ProjectilePrefab == null) return;

        // choose spawn transform
        Transform spawn = LaunchOffset ? LaunchOffset : (firePoint ? firePoint : transform);

        // direction from facing (independent of prefab rotation)
        Vector2 dir = facingRight ? Vector2.right : Vector2.left;

        // spawn a bit ahead (and slightly above) to avoid overlapping the player/ground
        const float forwardPad = 0.35f;
        const float upPad = 0.02f;
        Vector2 spawnPos = (Vector2)spawn.position + dir * forwardPad + Vector2.up * upPad;

        var proj = Instantiate(ProjectilePrefab, spawnPos, Quaternion.identity);

        // ignore collisions with our own colliders (prevents instant pop)
        var projCol = proj.GetComponent<Collider2D>();
        if (projCol)
        {
            foreach (var myCol in GetComponentsInChildren<Collider2D>())
                Physics2D.IgnoreCollision(projCol, myCol, true);
        }

        // drive the bullet by vector (no reliance on transform.right)
        proj.Launch(dir);
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    void OnDrawGizmosSelected()
    {
        if (!groundCheck) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
