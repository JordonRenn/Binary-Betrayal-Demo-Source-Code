using System.Collections;
using UnityEngine;

namespace SBG.VisualEffects
{
    /// <summary>
    /// Manages the muzzle flash effect for a weapon.
    /// </summary>
    public class MuzzleFlash : MonoBehaviour
    {
        [Header("Muzzle Flash Settings")]
        [SerializeField] private Renderer muzzleFlashRenderer;
        [SerializeField] private Light muzzleFlashLight;
        [SerializeField] private Vector3 defaultScale = new Vector3(10f, 10f, 10f);
        [SerializeField] private float defaultIntensity = 1.5f;
        [SerializeField] private float flashDecay = 0.3f;
        [SerializeField] private float flashHold = 0.1f;

        private Coroutine flashCoroutine;

        private void Start()
        {
            muzzleFlashRenderer.enabled = false;
            muzzleFlashLight.enabled = false;

            muzzleFlashRenderer.transform.localScale = defaultScale;
            muzzleFlashLight.intensity = defaultIntensity;
        }

        /// <summary>
        /// Triggers the muzzle flash effect.
        /// </summary>
        public void Flash()
        {
            if (flashCoroutine != null)
            {
                StopCoroutine(flashCoroutine);
            }
            flashCoroutine = StartCoroutine(FlashRoutine());
        }

        /// <summary>
        /// Coroutine to handle the muzzle flash effect.
        /// </summary>
        private IEnumerator FlashRoutine()
        {
            muzzleFlashRenderer.transform.localEulerAngles = new Vector3(0, 0, UnityEngine.Random.Range(0, 360));
            muzzleFlashRenderer.transform.localScale = defaultScale * UnityEngine.Random.Range(0.9f, 1.1f);

            muzzleFlashRenderer.enabled = true;
            muzzleFlashLight.enabled = true;

            yield return new WaitForSeconds(flashHold);

            float elapsedTime = 0f;

            while (elapsedTime < flashDecay)
            {
                elapsedTime += Time.deltaTime;
                muzzleFlashLight.intensity = Mathf.Lerp(defaultIntensity, 0f, elapsedTime / flashDecay * 2);
                muzzleFlashRenderer.material.color = new Color(1f, 1f, 1f, Mathf.Lerp(1f, 0f, elapsedTime / flashDecay));
                yield return null;
            }

            muzzleFlashLight.intensity = 0f; // Ensure the light is completely off


            yield return new WaitForSeconds(flashDecay + 0.25f);

            muzzleFlashRenderer.enabled = false;
            muzzleFlashLight.enabled = false;
            muzzleFlashRenderer.material.SetFloat("_Alpha", 1f);
        }
    }
}