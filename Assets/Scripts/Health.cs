using UnityEngine;

namespace Project.Scripts
{
    public class Health : MonoBehaviour
    {
        [SerializeField] private int maxHealth = 1;
        [SerializeField] private Sprite deathSprite;
        [SerializeField] private GameObject dropPrefab;
        
        private int _currentHealth;

        private void Start()
        {
            _currentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            _currentHealth -= amount;
            if (_currentHealth <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (dropPrefab != null)
            {
                Instantiate(dropPrefab, transform.position, Quaternion.identity);
            }

            // Create a new object for the corpse
            GameObject corpse = new GameObject(name + "_Corpse");
            corpse.transform.position = transform.position;
            corpse.transform.rotation = transform.rotation;
            corpse.transform.localScale = transform.localScale;

            // Add SpriteRenderer and configure it
            SpriteRenderer sr = corpse.AddComponent<SpriteRenderer>();
            sr.sprite = deathSprite;
            
            // Match sorting layer and order from the original object
            SpriteRenderer originalSR = GetComponent<SpriteRenderer>();
            if (originalSR != null)
            {
                sr.sortingLayerID = originalSR.sortingLayerID;
                sr.sortingOrder = originalSR.sortingOrder;
                sr.color = originalSR.color;
            }

            Destroy(gameObject);
        }
    }
}