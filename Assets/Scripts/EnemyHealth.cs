using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 2;
    int currentHealth;

    Animator animator;
    SpriteRenderer spriteRenderer;

    [Header("Hit Flash")]
    public Color flashColor = Color.red;
    public float flashDuration = 0.1f;
    Color originalColor;

    void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Flash red
        if (spriteRenderer != null)
            StartCoroutine(FlashRed());

        if (currentHealth <= 0)
        {
            // Wait until flash finishes before dying
            StartCoroutine(DieAfterFlash());
        }
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }

    IEnumerator DieAfterFlash()
    {
        // Wait for flash to complete
        yield return new WaitForSeconds(flashDuration);

        Debug.Log(gameObject.name + " died!");
        if (animator != null)
        {
            animator.SetTrigger("Die"); // Trigger death animation
        }
        else
        {
            Destroy(gameObject); // 🔴 Commented out as requested
        }
    }

    // Call this with an Animation Event at the end of the Enemy_Death animation
    public void DestroySelf()
    {
        Destroy(gameObject); // 🔴 Commented out as requested
    }
}
