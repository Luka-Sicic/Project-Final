using UnityEngine;

public class Shotgun : Weapon
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public int pelletCount = 5;
    public float spreadAngle = 15f;

    private void Start()
    {
        maxAmmo = 7;
        ammo = maxAmmo;
    }

    public override void Fire()
    {
        if (ammo <= 0) return;

        ammo--;

        NoiseManager.MakeNoise(transform.position, noiseRadius);

        // Fires only one bullet in the direction of the firePoint
GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = firePoint.right * bulletSpeed;
        }
    }
}
