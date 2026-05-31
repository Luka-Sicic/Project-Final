using UnityEngine;

public class PistolPickup : MonoBehaviour
{
    public GameObject pistolPrefab;
    public float pickupDistance = 2f;

    void Update()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.transform.position);
            if (distance < pickupDistance)
            {
                PickUp(player);
            }
        }
    }

    void PickUp(PlayerController player)
    {
        // Instantiate the pistol weapon that will be attached to the player
        GameObject pistolInstance = Instantiate(pistolPrefab);
        Pistol pistolScript = pistolInstance.GetComponent<Pistol>();
        
        player.EquipWeapon(pistolScript);

        // Trigger animation and set persistent state
        if (player.animator != null)
        {
            player.animator.SetTrigger("playerpistol");
            // Set persistent bool if it exists
            foreach (var param in player.animator.parameters)
            {
                if (param.name == "HasPistol")
                {
                    player.animator.SetBool("HasPistol", true);
                    break;
                }
            }
        }

        Destroy(gameObject); // Destroy the pickup object
    }
}
