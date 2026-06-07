using UnityEngine;
using System.Collections;

public abstract class Weapon : MonoBehaviour
{
    public int ammo;
    public int maxAmmo;
    public int spareReloads;
    public float reloadDuration = 2f;
    public bool IsReloading { get; protected set; }

    public abstract void Fire();

    public virtual void Reload()
    {
        if (spareReloads > 0 && ammo < maxAmmo && !IsReloading)
        {
            StartCoroutine(ReloadCoroutine());
        }
    }

    private IEnumerator ReloadCoroutine()
    {
        IsReloading = true;
        Debug.Log(gameObject.name + " reloading...");
        
        yield return new WaitForSeconds(reloadDuration);
        
        ammo = maxAmmo;
        spareReloads--;
        IsReloading = false;
        Debug.Log(gameObject.name + " reloaded. Spare reloads: " + spareReloads);
    }
}
