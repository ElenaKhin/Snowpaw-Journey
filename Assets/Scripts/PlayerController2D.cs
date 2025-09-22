using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [Header("Refs")]
    public Animator animator;              
    public LayerMask groundLayer;          
    public Transform firePoint;            

    [Header("Movement")]
    public float moveSpeed = 8f;
    public float jumpForce = 14f;
    public int maxAirJumps = 1;            

    [Header("Ground Check")]
    public float groundProbeDistance = 0.1f;
    public Vector2 groundProbeInset = new Vector2(0.05f, 0.05f);

    [Header("Jump Feel")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("Respawn")]
    public Transform respawnPoint;         
    public float fallYThreshold = -10f;    
    public float respawnInvulnTime = 0.2f;

    [Header("Attack")]
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public LayerMask enemyLayers;

    [Header("Game Manager")]
    public GameManager GameManager;

    [Header("Audio")]
    [SerializeField] private AudioManager audioManager;

    Rigidbody2D rb;
    Collider2D col;

    int airJumpsUsed = 0;
    float xInput;
    bool jumpPressedEdge;
    bool facingRight = true;
    bool isGrounded;

    float coyoteTimer = 0f;
    float jumpBufferTimer = 0f;
    bool respawning = false;

    public int maxHealth = 10;
    public int currentHealth;

    public HealthBar healthBar;
    

    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

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

        if (!audioManager)
        audioManager = FindObjectOfType<AudioManager>();
    }

    void Update()
    {
        xInput = Input.GetAxisRaw("Horizontal");

        // Ground check
        isGrounded = CheckGrounded();

        // Reset air jumps when grounded
        if (isGrounded)
            airJumpsUsed = 0;

        // Timers
        if (isGrounded)
            coyoteTimer = coyoteTime;
        else
            coyoteTimer -= Time.deltaTime;

        if (jumpBufferTimer > 0f)
            jumpBufferTimer -= Time.deltaTime;

        // Jump input
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedEdge = true;
            jumpBufferTimer = jumpBufferTime;
        }

        // Flip player
        if (xInput > 0 && !facingRight) Flip();
        else if (xInput < 0 && facingRight) Flip();

        // Auto respawn
        if (!respawning && transform.position.y < fallYThreshold)
            DoRespawn();

        // Animator
        if (animator)
        {
            if (HasParam("Speed", AnimatorControllerParameterType.Float))
                animator.SetFloat("Speed", Mathf.Abs(xInput));
            if (HasParam("Grounded", AnimatorControllerParameterType.Bool))
                animator.SetBool("Grounded", isGrounded);
            if (HasParam("YVel", AnimatorControllerParameterType.Float))
                animator.SetFloat("YVel", rb.velocity.y);
        }
        // Attack input (Enter key)
        // Attack input (Enter key)
        if (Input.GetKeyDown(KeyCode.Return))
        {
            DoAttack();
        }
    }

    void FixedUpdate()
    {
        // Horizontal movement
        rb.velocity = new Vector2(xInput * moveSpeed, rb.velocity.y);

        // Jump logic
        if (jumpPressedEdge)
        {
            bool canJump = isGrounded || airJumpsUsed < maxAirJumps || coyoteTimer > 0f;

            if (canJump)
            {
                DoJump();

                if (!isGrounded && coyoteTimer <= 0f)
                    airJumpsUsed++;

                jumpPressedEdge = false;
                jumpBufferTimer = 0f;
            }
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
    void DoAttack()
    {
        if (animator && HasParam("Attack", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Attack");

        audioManager.PlayAttack();

        // Detect enemies in range
        Collider2D[] hits = Physics2D.OverlapCircleAll(firePoint.position, attackRange, enemyLayers);

        foreach (Collider2D hit in hits)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
                audioManager.PlayEnemyHurt();
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Max(currentHealth, 0); // Prevent negative health

        // Update health bar
        if (healthBar != null)
            healthBar.SetHealth(currentHealth);

        audioManager.PlayHurt();

        // Play hurt animation if available
        if (animator && HasParam("Hurt", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Hurt");

        // Optional: check death
        if (currentHealth <= 0)
            Die();
    }

    void Die()
    {
        Debug.Log("Player died!");

        rb.velocity = Vector2.zero;
        this.enabled = false; // disable player controls

        // Play death animation
        if (animator && HasParam("Dead", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Dead");
        audioManager.PlayDeath();

        // Delay GameOver until after animation (e.g., 1s)
        StartCoroutine(ShowGameOverAfterDelay(0.8f));
    }

    IEnumerator ShowGameOverAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (GameManager != null)
        {
            GameManager.GameOver();
        }
    }

    bool CheckGrounded()
    {
        Bounds b = col.bounds;
        Vector2 size = new Vector2(
            Mathf.Max(0.01f, b.size.x - groundProbeInset.x),
            Mathf.Max(0.01f, b.size.y - groundProbeInset.y)
        );

        RaycastHit2D hit = Physics2D.BoxCast(b.center, size, 0f, Vector2.down, groundProbeDistance, groundLayer);

#if UNITY_EDITOR
        Debug.DrawRay(b.center, Vector2.down * groundProbeDistance, hit.collider != null ? Color.green : Color.red);
#endif

        return hit.collider != null;
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 s = transform.localScale;
        s.x *= -1f;
        transform.localScale = s;
    }

    public void DoRespawn()
    {
        StartCoroutine(CoRespawn());
    }

    IEnumerator CoRespawn()
    {
        respawning = true;
        Vector3 target = respawnPoint ? respawnPoint.position
                                      : new Vector3(transform.position.x, 0f, transform.position.z);

        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        airJumpsUsed = 0;
        coyoteTimer = 0f;
        jumpBufferTimer = 0f;

        transform.position = target;

        if (respawnInvulnTime > 0f && col)
        {
            col.enabled = false;
            yield return new WaitForSeconds(respawnInvulnTime);
            col.enabled = true;
        }

        respawning = false;

        if (animator && HasParam("Respawn", AnimatorControllerParameterType.Trigger))
            animator.SetTrigger("Respawn");
    }

    bool HasParam(string name, AnimatorControllerParameterType type)
    {
        if (!animator) return false;
        foreach (var p in animator.parameters)
            if (p.name == name && p.type == type) return true;
        return false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Final"))
        {
            Debug.Log("Player reached the final (collision)!");
            if (GameManager != null)
                GameManager.WinGame();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Final"))
        {
            Debug.Log("Player reached the final (trigger)!");
            if (GameManager != null)
                GameManager.WinGame();
        }
    }
}
