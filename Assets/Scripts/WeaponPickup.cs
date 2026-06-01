using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    [Header("Settings")]
    public GameObject weaponPrefab;
    public float pickupDistance = 2f;
    
    [Header("Animation")]
    public string animTrigger;
    public string animBool;

    [Header("UI")]
    public GameObject canvasPrompt;

    private PlayerController player;
    private static WeaponPickup currentInteractable;

    void Start()
    {
        player = Object.FindAnyObjectByType<PlayerController>();
        if (canvasPrompt != null) canvasPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.transform.position);
        bool inRange = distance < pickupDistance;

        if (inRange)
        {
            if (currentInteractable == null || currentInteractable == this)
            {
                currentInteractable = this;
            }
            else
            {
                float currentDist = Vector2.Distance(currentInteractable.transform.position, player.transform.position);
                if (distance < currentDist)
                {
                    // Swapping to a closer one
                    if (currentInteractable.canvasPrompt != null) 
                        currentInteractable.canvasPrompt.SetActive(false);
                    currentInteractable = this;
                }
            }
        }
        else if (currentInteractable == this)
        {
            currentInteractable = null;
            if (canvasPrompt != null) canvasPrompt.SetActive(false);
        }

        if (currentInteractable == this)
        {
            if (canvasPrompt != null && !canvasPrompt.activeSelf) 
                canvasPrompt.SetActive(true);
            
            if (Input.GetKeyDown(KeyCode.E))
            {
                PickUp();
            }
        }
    }

    void PickUp()
    {
        if (currentInteractable == this)
        {
            currentInteractable = null;
        }

        if (canvasPrompt != null) canvasPrompt.SetActive(false);

        if (weaponPrefab == null)
        {
            Debug.LogWarning("WeaponPickup: No weapon prefab assigned!");
            return;
        }

        GameObject weaponInstance = Instantiate(weaponPrefab);
        Weapon weaponScript = weaponInstance.GetComponent<Weapon>();
        
        if (weaponScript != null)
        {
            player.EquipWeapon(weaponScript);
        }

        // Handle Animations
        if (player.animator != null)
        {
            if (!string.IsNullOrEmpty(animTrigger))
                player.animator.SetTrigger(animTrigger);
            
            if (!string.IsNullOrEmpty(animBool))
            {
                foreach (var param in player.animator.parameters)
                {
                    if (param.name == animBool)
                    {
                        player.animator.SetBool(animBool, true);
                        break;
                    }
                }
            }
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (currentInteractable == this)
        {
            currentInteractable = null;
        }
        if (canvasPrompt != null) canvasPrompt.SetActive(false);
    }
}
