using UnityEngine;

public class ShotgunPickup : MonoBehaviour
{
    public GameObject shotgunPrefab;
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
        // Instantiate the shotgun weapon that will be attached to the player
        GameObject shotgunInstance = Instantiate(shotgunPrefab);
        Shotgun shotgunScript = shotgunInstance.GetComponent<Shotgun>();
        
        player.EquipWeapon(shotgunScript);

        // Trigger animation and set persistent state
        if (player.animator != null)
        {
            player.animator.SetTrigger("playershotgun");
            // Set persistent bool if it exists
            foreach (var param in player.animator.parameters)
            {
                if (param.name == "HasShotgun")
                {
                    player.animator.SetBool("HasShotgun", true);
                    break;
                }
            }
        }

        Destroy(gameObject); // Destroy the pickup object
    }
}
