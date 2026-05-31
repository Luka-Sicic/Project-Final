using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Weapon weapon;
    public Rigidbody2D rb;
    public Animator animator;

    Vector2 moveDirection;
    Vector2 mousePosition;

    // Animator parameter hash
    private static readonly int IsWalkingHash = Animator.StringToHash("IsWalking");

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

        // 2. Sample Mouse Position
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 3. Update Animator
        if (animator != null)
        {
            animator.SetBool(IsWalkingHash, moveDirection.magnitude > 0);
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