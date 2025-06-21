using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Timeline;
using System.Collections;


[RequireComponent(typeof(Animator))]
public class SpriteVFX : MonoBehaviour
{

    private Animator animator;

    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f)
        {
            Destroy(gameObject);
        }
    }
}
