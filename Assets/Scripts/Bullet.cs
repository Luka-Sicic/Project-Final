using UnityEngine;
using Project.Scripts;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        if (collision.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(1);
        }

        Destroy(gameObject);
    }
}
