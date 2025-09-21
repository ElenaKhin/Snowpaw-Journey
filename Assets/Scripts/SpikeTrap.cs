using UnityEngine;

public class SpikeTrap : MonoBehaviour
{
    public int damage = 1; // damage to player

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player touched the trap
        PlayerController2D player = other.GetComponent<PlayerController2D>();
        if (player != null)
        {
            player.TakeDamage(damage);
        }
    }
}
