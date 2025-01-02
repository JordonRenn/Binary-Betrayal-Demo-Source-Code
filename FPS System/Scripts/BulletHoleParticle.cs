using UnityEngine;
using System.Collections;

public class BulletHoleParticle : MonoBehaviour
{
    [Tooltip("The total lifetime of the decal in seconds.")]
    [SerializeField] float lifeTime = 3f;

    private void Start()
    {
        StartCoroutine(LifeCycle(lifeTime));
    }

    private IEnumerator LifeCycle(float _lifeTime)
    {        
        yield return new WaitForSeconds(_lifeTime);       

        Destroy(gameObject);
    }
}
