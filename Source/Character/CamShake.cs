using System.Collections;
using UnityEngine;

namespace BinaryBetrayal.CameraControl
{
    /* 
        First Person Controller Hierarchy:

        - Character Controller (CharacterMovement.cs)
            - FPS_Cam (FirstPersonCamController.cs + CamShake.cs)           <--- THIS SCRIPT
                - FPS System (FPSS_Main.cs)
                    - FPS_Interaction (FirstPersonInteraction.cs) 
                    - FPS_WeaponObjectPool (FPSS_Pool.cs)                   
                        - POS_GUN_AUDIO
                        - 0_0_Ak-47 (Gun_AK47.cs)
                            - AK_47
                                - MuzzleFlash (MuzzleFlash.cs)
                        - 0_1_SniperRifle (FPSS_WeaponSlotObject.cs)        // Need to make "Gun_SniperRifle.cs"
                        - 1_0_HandGun (Gun_HandGun.cs)
                            - HandGun
                                - MuzzleFlash (MuzzleFlash.cs)
                        - 1_1_ShotGun (FPSS_WeaponSlotObject.cs)            // Need to make "Gun_ShotGun.cs"
                        - 2_0_Knife (FPSS_WeaponSlotObject.cs)              // Need to make "Melee_Knife.cs"
                        - 3_0_Grenade (FPSS_WeaponSlotObject.cs)            // Need to make "Grenade.cs"
                        - 3_1_FlashGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "FlashGrenade.cs"
                        - 3_2_SmokeGrenade (FPSS_WeaponSlotObject.cs)       // Need to make "SmokeGrenade.cs"
                        - 4_0_Unarmed (FPSS_WeaponSlotObject.cs)            // Need to make "Unarmed.cs"
    */

    public class CamShake : MonoBehaviour
    {
        [SerializeField] float shakeDuration = 0.3f;
        [Tooltip("Intensity multiplier")]
        [SerializeField][Range(0, 1)] float shakeIntensity;
        [SerializeField] float rotationIntensity;
        [SerializeField] AnimationCurve curve;

        public void Shake(float intensity, float rotationIntensity = 0f)
        {

            StartCoroutine(Shaking(intensity, rotationIntensity));
        }

        IEnumerator Shaking(float intensity, float rotationIntensity)
        {
            shakeIntensity = intensity;
            Vector3 startPosition = transform.localPosition;
            float elapsedTime = 0f;

            while (elapsedTime < shakeDuration)
            {
                elapsedTime += Time.deltaTime;
                float strengthCurve = curve.Evaluate(elapsedTime / shakeDuration);
                transform.localPosition = startPosition + Random.insideUnitSphere * strengthCurve * shakeIntensity;
                float zRotation = Random.Range(-1f, 1f) * strengthCurve * rotationIntensity;
                transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, zRotation);
                yield return null;
            }

            transform.localPosition = new Vector3(0, 1, 0);
            transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
        }
    }
}