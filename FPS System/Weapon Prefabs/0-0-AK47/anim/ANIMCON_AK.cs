using UnityEngine;

public class ANIMCON_AK : MonoBehaviour
{
    private Animator animator; 

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void Fire()
    {
        animator.SetTrigger("Fire");
    }

    public void Reload()
    {
        animator.SetTrigger("Reload");
    }

    public void Arm()
    {
        animator.SetTrigger("Arm");
    }

    public void Disarm()
    {
        animator.SetTrigger("Disarm");
    }

    public void Idle()
    {
        animator.SetTrigger("Idle");
    } 
}
