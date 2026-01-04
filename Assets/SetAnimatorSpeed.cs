using UnityEngine;

public class SetAnimatorSpeed : MonoBehaviour
{
    public Animator referenceAnimator, thisAnimator;
    public float multiplier = 1;
    void Update()
    {
        thisAnimator.speed = referenceAnimator.velocity.magnitude * multiplier;
    }
}
