using UnityEngine;

public class GunShell : MonoBehaviour
{
    Rigidbody rb;
    [SerializeField] float force;
    [SerializeField] float shellDespawnTime;
    
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        Eject();
    }

    void Eject()
    {
        rb.AddRelativeForce(new Vector3(25,90,25) * force);
        transform.parent = null;
        Invoke("DespawnShell", shellDespawnTime);
    }
    
    void DespawnShell ()
    {
        Destroy(gameObject);
    }
}
