using UnityEngine;

public class Bullet : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        // Ignore collisions with the player
        if (collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        Destroy(gameObject);
    }
}
