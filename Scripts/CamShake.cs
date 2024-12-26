using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    [SerializeField] float shakeDuration = 0.5f;
    [Tooltip("Intensity multiplier")]
    [SerializeField][Range(0,1)] float shakeIntensity;
    [SerializeField] AnimationCurve curve;

    public void Shake(float intensity)
    {
        
        StartCoroutine(Shaking(intensity));
    }

    IEnumerator Shaking(float intensity)
    {
        shakeIntensity = intensity;
        Vector3 startPosition = transform.localPosition;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            float strengthCurve = curve.Evaluate(elapsedTime / shakeDuration);
            transform.localPosition = startPosition + Random.insideUnitSphere * strengthCurve * shakeIntensity;
            yield return null;
        }

        this.transform.localPosition = Vector3.zero;
    }
}
