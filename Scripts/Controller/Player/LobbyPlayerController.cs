using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobbyPlayerController : BaseController
{
    [SerializeField]
    Animator doorAnimator;
    public bool MoveToDestination(Vector3 destination, float stopDistance)
    {
        if (Vector3.Distance(transform.position, destination) > stopDistance)
        {
            //감속
            float minSpeed = 0.8f;
            float runSpeed = Mathf.Max(Vector3.Distance(transform.position, destination), minSpeed);
            //이동
            Vector3 dir = (destination - transform.position).normalized;
            dir = new Vector3(dir.x, 0, dir.z);
            if (Rotation(dir))
            {
                rb.velocity = dir * runSpeed * 50 * Time.fixedDeltaTime;
                animator.SetFloat(MOVE_HASH, runSpeed);
            }
            return false;
        }
        rb.velocity = Vector3.zero;
        animator.SetFloat(MOVE_HASH, 0);
        return true;
    }

    public void AnimationAttack()
    {
        animator.SetTrigger(ATTACK_HASH);
    }

    public void KickAttack()
    {
        doorAnimator.SetBool("Foward", true);
        doorAnimator.SetBool("Open", true);
    }
}
