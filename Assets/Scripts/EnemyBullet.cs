using UnityEngine;
using Project.Scripts;

namespace Project.Scripts
{
    public class EnemyBullet : MonoBehaviour
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            // Ignore collisions with enemies
            if (collision.gameObject.CompareTag("Enemy"))
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
}
