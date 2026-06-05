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

    public void Kick(Vector2 direction)
    {
        if (isLocked) UnlockDoor();
        _hasBeenOpened = true;
        
        if (rb != null)
        {
            rb.AddForceAtPosition(direction * kickForce, transform.position + (Vector3)direction * 0.5f, ForceMode2D.Impulse);
            lethalTimer = 1.0f; // Door is lethal for 1.0 seconds after being kicked
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (lethalTimer > 0)
        {
            // Check if we hit an enemy
            Project.Scripts.Health health = collision.gameObject.GetComponentInParent<Project.Scripts.Health>();
            if (health != null)
            {
                // Kill them
                health.TakeDamage(999);
                Debug.Log("Enemy smashed by door!");
            }
        }
    }
}
