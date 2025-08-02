using System.Collections;
using UnityEngine;

public class CamShake : MonoBehaviour
{
    [SerializeField] float shakeDuration = 0.3f;
    [Tooltip("Intensity multiplier")]
    [SerializeField][Range(0,1)] float shakeIntensity;
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

        transform.localPosition = new Vector3(0,1,0);
        transform.localRotation = Quaternion.Euler(transform.localRotation.eulerAngles.x, transform.localRotation.eulerAngles.y, 0);
    }
}
