using UnityEngine;
using UnityEditor;

[ExecuteAlways] // Ensures the script runs in edit mode
public class ForceIKEvaluation : MonoBehaviour
{
    private Animator animator;

    void OnEnable()
    {
        animator = GetComponent<Animator>();
        if (animator)
        {
            // Force Animator to evaluate IK in the Editor
            animator.updateMode = AnimatorUpdateMode.Fixed;
            animator.animatePhysics = true;
            EditorApplication.update += UpdateAnimator;
        }
    }

    void OnDisable()
    {
        if (animator)
        {
            animator.updateMode = AnimatorUpdateMode.Normal;
            animator.animatePhysics = false;
            EditorApplication.update -= UpdateAnimator;
        }

        
    }

    void UpdateAnimator()
    {
        if (!Application.isPlaying)
        {
            animator.Rebind();
            animator.Update(0f);
        }
    }
}