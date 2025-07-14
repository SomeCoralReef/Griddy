using UnityEngine;

public class PlayerAnimationControllerScript : MonoBehaviour
{
    public Animator animator;
    public string currentAnimationState = "Idle";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void PlayerPrepare(bool isPreparing)
    {
        currentAnimationState = isPreparing ? "Preparing" : "Idle";
        animator.SetBool("isPreparing", isPreparing);
    }

    public void PlayAttack() 
    {
        animator.SetTrigger("isAttacking");
    }
}
