using UnityEngine;
using Project.Scripts;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Weapon weapon;
    public Rigidbody2D rb;
    public Animator animator;

    [Header("Kick Settings")]
    public float kickRange = 0.8f;
    public int kickDamage = 0; // Direct kick is now non-lethal
    public float kickForce = 5f;
    public LayerMask kickLayers;

    Vector2 moveDirection;
    Vector2 mousePosition;

    // Animator parameter hash
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");
    private static readonly int PlayerKickHash = Animator.StringToHash("playerkick");

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (animator == null) animator = GetComponent<Animator>();
        
        // Ensure Rigidbody is set to Interpolate for smooth movement
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        // 1. Gather Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetMouseButtonDown(0) && weapon != null)
            weapon.Fire();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Kick();
        }

        // 2. Sample Mouse Position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 3. Update Animator
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, moveDirection.magnitude > 0);
        }
    }

    public void Kick()
    {
        Debug.Log("Player Kicked!");
if (animator != null)
        {
            animator.SetTrigger(PlayerKickHash);
        }

        // The origin of the circle should be the player's position + (lookDirection * rangeOffset).
        // Using transform.right as lookDirection and 0.5f as a reasonable rangeOffset.
        Vector2 kickOrigin = (Vector2)transform.position + (Vector2)transform.right * 0.5f;
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(kickOrigin, kickRange, kickLayers);

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.TryGetComponent<Door>(out Door door))
            {
                door.Kick(transform.right);
            }

            if (hitCollider.TryGetComponent<EnemyAStarFollow>(out EnemyAStarFollow enemy))
            {
                enemy.Stun(1f);
                
                // Push back logic
                Rigidbody2D enemyRb = hitCollider.GetComponent<Rigidbody2D>();
                if (enemyRb != null)
                {
                    enemyRb.AddForce(transform.right * kickForce, ForceMode2D.Impulse);
                }
            }

            // Optional: Still allow dealing damage if kickDamage > 0, 
            // but the prompt says "only kill enemies when the player kicks a door"
            if (kickDamage > 0 && hitCollider.TryGetComponent<Health>(out Health health))
            {
                health.TakeDamage(kickDamage);
            }
        }
    }

    public void EquipWeapon(Weapon newWeapon)
    {
        if (weapon != null)
        {
            Destroy(weapon.gameObject);
        }

        weapon = newWeapon;
        weapon.transform.SetParent(transform);
        weapon.transform.localPosition = Vector3.zero;
        weapon.transform.localRotation = Quaternion.identity;
    }

    void FixedUpdate()
    {
        // 4. Apply Velocity
        rb.linearVelocity = moveDirection * moveSpeed;

        // 5. Apply Rotation in FixedUpdate
        Vector2 lookDirection = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }
}