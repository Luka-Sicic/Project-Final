using UnityEngine;

public class Gun : Weapon
{
    public GameObject Bullet;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public override void Fire()
    {
        GameObject bullet = Instantiate(
            Bullet,
            firePoint.position,
            firePoint.rotation
        );

        bullet.GetComponent<Rigidbody2D>().linearVelocity =
            firePoint.right * bulletSpeed;
    }
}