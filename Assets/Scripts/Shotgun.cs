using UnityEngine;

public class Shotgun : Weapon
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public int pelletCount = 5;
    public float spreadAngle = 15f;

    public override void Fire()
    {
        for (int i = 0; i < pelletCount; i++)
        {
            float angleOffset = Random.Range(-spreadAngle, spreadAngle);
            // Apply rotation to the firePoint's up direction
            Quaternion rotation = Quaternion.Euler(0, 0, angleOffset);
            Vector2 bulletDirection = rotation * firePoint.up;

            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation * rotation);
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = bulletDirection * bulletSpeed;
            }
        }
    }
}
