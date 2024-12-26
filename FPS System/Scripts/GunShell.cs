using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//FIX LIST:
//1. shells fall through floor

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
