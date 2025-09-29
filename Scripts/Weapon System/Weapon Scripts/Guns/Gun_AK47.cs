using UnityEngine;
using System.Collections;

public class Gun_AK47 : WPO_Gun, IWPO_Gun //inherits from FPSS_WeaponSlotObject and implements WPO_Gun
{
    public override void Fire()
    {
        // Debug.Log("Fire AK47");
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
            animator.Play("Fire", 0, 0f); // Play from beginning with no blending
            
            // Decrement ammo count BEFORE calling FireHitScan
            currentClip--;
            
            FireHitScan();

            // FPSS_ReticleSystem.Instance.GunFire(reticleFallOffSpeed);
            ReticleSystem.Instance?.Impulse(0.5f, 0.5f);
            
            ApplySpread();
            
            camShake.Shake(camShakeIntensity, 2f);

            yield return new WaitForSeconds(fireRate);
            canFire = true;

            if (InputHandler.Instance.FireInput && !isReloading)
            {
                StartCoroutine(FireBullet());
            }
        }
        else
        {
            PlaySfx(sfx_Empty, pos_GunAudio.position);
        }
    }

    public override void Reload()
    {
        // Debug.Log("Reload AK47");
        if (isReloading) return;
        if (!canReload) return;

        StartCoroutine(ReloadSequence());
    }

    private IEnumerator ReloadSequence()
    {
        isReloading = true;
        yield return ReloadWeapon();
        isReloading = false;
        canReload = true;
    }
}
