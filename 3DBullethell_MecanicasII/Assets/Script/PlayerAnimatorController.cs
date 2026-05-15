using UnityEngine;

public class PlayerAnimatorController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public Rigidbody rb;

    [Header("Animator Parameters")]
    public string movingParameter = "isMoving";
    public string attackParameter = "attack";

    [Header("State Names")]
    public string attackStateName = "attack";

    [Header("Settings")]
    public float moveThreshold = 0.05f;
    public bool forceAttackState = true;
    public float attackTransitionTime = 0.02f;

    private int movingHash;
    private int attackHash;

    private void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();

        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (rb == null)
            rb = GetComponentInParent<Rigidbody>();

        movingHash = Animator.StringToHash(movingParameter);
        attackHash = Animator.StringToHash(attackParameter);
    }

    private void Update()
    {
        UpdateMovementAnimation();
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || rb == null) return;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.sqrMagnitude > moveThreshold * moveThreshold;

        animator.SetBool(movingHash, isMoving);
    }

    public void PlayAttack()
    {
        if (animator == null)
        {
            Debug.LogWarning("PlayerAnimatorController: no hay Animator asignado.");
            return;
        }

        Debug.Log("Animación attack lanzada");

        animator.ResetTrigger(attackHash);
        animator.SetTrigger(attackHash);

        if (forceAttackState)
        {
            animator.CrossFadeInFixedTime(
                attackStateName,
                attackTransitionTime,
                0,
                0f
            );
        }
    }
}