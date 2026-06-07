using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Door : MonoBehaviour, IInteractable
{
    [Header("Door Settings")]
    public bool isLocked = false;
    public float kickForce = 500f;

    private Rigidbody2D rb;
    private HingeJoint2D hinge;
    private JointAngleLimits2D closedLimits;
    private JointAngleLimits2D openLimits;
    private float lethalTimer = 0f;
    private bool _hasBeenOpened = false;

    void Start()
{
        rb = GetComponent<Rigidbody2D>();
        hinge = GetComponent<HingeJoint2D>();
        
        if (hinge != null)
        {
            closedLimits = hinge.limits;
        }

        if (isLocked)
        {
            LockDoor();
        }
    }

    void Update()
    {
        if (lethalTimer > 0)
        {
            lethalTimer -= Time.deltaTime;
        }
    }

    public void Interact()
    {
        if (_hasBeenOpened) return;

        if (isLocked)
        {
            UnlockDoor();
            Debug.Log("Door unlocked!");
        }
    }

    public string GetInteractPrompt()
    {
        if (_hasBeenOpened) return "";
        return isLocked ? "Unlock Door" : "";
    }

    public void LockDoor()
    {
        isLocked = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
    }

    public void UnlockDoor()
    {
        isLocked = false;
        _hasBeenOpened = true;
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void Kick(Vector2 kickerPosition)
    {
        if (isLocked) UnlockDoor();
        _hasBeenOpened = true;
        
        if (rb != null)
        {
            // Calculate direction from kicker to door center
            Vector2 doorCenter = rb.worldCenterOfMass;
            Vector2 direction = (doorCenter - kickerPosition).normalized;
            
            // Apply force at the center of mass in the direction away from the kicker.
            // Since the door is hinged, this will produce torque that swings the door away.
            rb.AddForceAtPosition(direction * kickForce, doorCenter, ForceMode2D.Impulse);
            lethalTimer = 1.0f; // Door is lethal for 1.0 seconds after being kicked
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (lethalTimer <= 0) return;

        // Don't kill other doors
        if (collision.gameObject.GetComponentInParent<Door>() != null) return;

        // Don't kill the player
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.GetComponentInParent<PlayerController>() != null) return;

        Project.Scripts.Health health = collision.gameObject.GetComponentInParent<Project.Scripts.Health>();
        if (health != null)
        {
            health.TakeDamage(999);
            Debug.Log("Enemy smashed by door!");
        }
    }
}
