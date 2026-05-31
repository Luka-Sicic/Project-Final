using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Gun gun;
    public Rigidbody2D rb;

    Vector2 moveDirection;
    Vector2 mousePosition; // This will now correctly store the value for FixedUpdate

    void Update()
    {
        // 1. Gather Input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        if (Input.GetMouseButtonDown(0))
            gun.Fire();

        // 2. Sample Mouse Position (Remove 'Vector2' here to fix variable shadowing)
        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void FixedUpdate()
    {
        // 3. Apply Velocity
        rb.linearVelocity = moveDirection * moveSpeed;

        // 4. Apply Rotation in FixedUpdate
        Vector2 lookDirection = mousePosition - rb.position;
        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;
        rb.MoveRotation(angle);
    }
}