using UnityEngine;
using System.Collections;

public class Gun_Handgun : WPO_Gun, IWPO_Gun //inherits from FPSS_WeaponSlotObject and implements WPO_Gun
{
    public override void Fire()
    {
        // Debug.Log("Fire Handgun");
        if (isActive && !isReloading)
        {
            StartCoroutine(FireBullet());
        }
    }

    private IEnumerator FireBullet()
    {
        if (currentClip > 0 && canFire)
        {
            canFire = false;

            PlaySfx(sfx_Fire, pos_GunAudio.position);
            animator.Play("Fire", 0, 0f); // Play from beginning with specific layer

            // Decrement ammo count BEFORE calling FireHitScan
            currentClip--;

            FireHitScan();

            ReticleSystem.Instance?.Impulse(0.1f, 0.5f);

            ApplySpread();

            camShake.Shake(camShakeIntensity, 2f);

            yield return new WaitForSeconds(fireRate);
            canFire = true;
        }
        else
        {
            PlaySfx(sfx_Empty, pos_GunAudio.position);
        }
    }

    public override void Reload()
    {
        // Debug.Log("Reload Handgun");
        if (isReloading) return;
        if (!canReload) return;

        StartCoroutine(ReloadSequence());
    }

    private IEnumerator ReloadSequence()
    {
        yield return ReloadWeapon();
        isReloading = false;
        canReload = true;
    }
}
