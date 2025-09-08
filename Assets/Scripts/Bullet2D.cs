// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Bullet2D : MonoBehaviour
// {
//     public float lifeTime = 2.5f;
//     public int damage = 1;

//     Rigidbody2D rb;

//     void Awake()
//     {
//         rb = GetComponent<Rigidbody2D>();
//         rb.gravityScale = 0f;
//         GetComponent<Collider2D>().isTrigger = true;
//     }

//     public void Launch(Vector2 velocity)
//     {
//         rb.velocity = velocity;
//     }

//     void OnEnable() => Invoke(nameof(Kill), lifeTime);
//     void Kill() => Destroy(gameObject);

//     void OnTriggerEnter2D(Collider2D other)
//     {
//         // Example damage hook:
//         // other.GetComponent<EnemyHealth>()?.TakeDamage(damage);

//         // Despawn on hitting ground / enemy / wall
//         if (other.gameObject.layer == LayerMask.NameToLayer("Ground") ||
//             other.CompareTag("Enemy") || other.CompareTag("Wall"))
//         {
//             Destroy(gameObject);
//         }
//     }
// }
