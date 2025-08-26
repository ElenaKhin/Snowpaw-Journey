using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;         // optional
    public Transform groundCheck;     // empty child at feet
    public LayerMask groundLayer;     // set to Ground layer
    public Transform firePoint;       // empty child at barrel/front
    public GameObject bulletPrefab;   // prefab with Bullet2D script

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public float groundCheckRadius = 0.15f;
    public int maxAirJumps = 0;       // set to 1 for double jump, etc.

    [Header("Shooting")]
    public float fireRate = 8f;       // bullets per second
    public float bulletSpeed = 16f;

    Rigidbody2D rb;
    int airJumpsUsed = 0;
    float xInput;
    bool jumpPressed;
    bool facingRight = true;
    float nextShotTime;

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

        // shoot input (hold)
        if (Input.GetButton("Fire1")) TryShoot();

        // flip visual
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
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    void TryShoot()
    {
        if (Time.time < nextShotTime || bulletPrefab == null || firePoint == null) return;
        nextShotTime = Time.time + 1f / fireRate;

        GameObject b = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        float dir = facingRight ? 1f : -1f;

        // launch
        var bullet = b.GetComponent<Bullet2D>();
        if (bullet) bullet.Launch(new Vector2(dir * bulletSpeed, 0f));
        else
        {
            // fallback if no script: push via RB2D
            var rb2 = b.GetComponent<Rigidbody2D>();
            if (rb2) rb2.velocity = new Vector2(dir * bulletSpeed, 0f);
        }
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
