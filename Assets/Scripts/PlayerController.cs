using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public Gun gun;
    public Rigidbody2D rb;

    Vector2 moveDirection;
    Vector2 mousePosition;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        if (Input.GetMouseButtonDown(0))
        {
            gun.Fire();
        }

        moveDirection = new Vector2(moveX, moveY).normalized;

        mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * moveSpeed;

        Vector2 lookDirection = mousePosition - rb.position;

        float angle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg;

        rb.rotation = angle;
    }
}