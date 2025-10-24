using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;
using System.Collections;
using Unity.VisualScripting;

namespace SBG.VisualEffects
{
    public class BulletHoleDecal : MonoBehaviour
    {
        [Tooltip("The total lifetime of the decal in seconds.")]
        [SerializeField] float lifeTime = 10f;

        [Tooltip("The time it takes for the decal to fade out. This should be less than the lifetime as it is subtracted from the total lifetime.")]
        [SerializeField] float fadeTime = 5f;

        private DecalProjector decalProjector;

        private void Start()
        {
            StartCoroutine(LifeCycle(lifeTime, fadeTime));
        }

        private IEnumerator LifeCycle(float _lifeTime, float _fadeTime)
        {
            yield return new WaitForSeconds(_lifeTime - _fadeTime);

            float timer = 0f;
            float startOpacity = 1f;

            decalProjector = GetComponent<DecalProjector>(); // Corrected assignment

            while (timer < _fadeTime)
            {
                timer += Time.deltaTime;
                float lerpValue = Mathf.Lerp(startOpacity, 0f, timer / _fadeTime);
                decalProjector.fadeFactor = lerpValue;
                yield return null; // Ensure the loop waits for the next frame
            }

            Destroy(gameObject);
        }
    }
}