using UnityEngine;

public class Gun : MonoBehaviour
{
    public GameObject Bullet;
    public Transform firePoint;
    public float bulletSpeed = 20f;

    public void Fire()
    {
        GameObject bullet = Instantiate(
            Bullet,
            firePoint.position,
            firePoint.rotation
        );

        bullet.GetComponent<Rigidbody2D>().linearVelocity =
            firePoint.up * bulletSpeed;
    }
}