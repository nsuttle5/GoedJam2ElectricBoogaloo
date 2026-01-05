using UnityEngine;

public class SetAnimatorSpeed : MonoBehaviour
{
    public Animator referenceAnimator, thisAnimator;
    public float multiplier = 1;
    private bool stop;
    public void Stop()
    {
        stop = true;
    }
    void Update()
    {
        if (stop)
        {
            thisAnimator.speed = 1;
            return;
        }
        thisAnimator.speed = referenceAnimator.velocity.magnitude * multiplier;
    }
}
