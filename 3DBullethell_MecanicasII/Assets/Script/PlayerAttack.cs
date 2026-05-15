using System.Collections;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Input")]
    public KeyCode attackKeyKeyboard = KeyCode.J;
    public KeyCode attackKeyGamepad = KeyCode.JoystickButton0;
    // En muchos mandos PlayStation, cuadrado suele ser JoystickButton0

    [Header("Attack Settings")]
    public float attackDuration = 0.25f;
    public float hitboxStartTime = 0.06f;
    public float hitboxActiveTime = 0.12f;
    public float attackCooldown = 0.15f;
    public float attackMoveLockTime = 0.18f;

    [Header("References")]
    public PlayerMovement playerMovement;
    public Transform swordPivot;
    public SwordHitbox swordHitbox;
    public PlayerAnimatorController playerAnimator;

    public bool CanAttack { get; set; } = true;

    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerAnimator == null)
            playerAnimator = GetComponent<PlayerAnimatorController>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInChildren<PlayerAnimatorController>();

        if (playerAnimator == null)
            playerAnimator = GetComponentInParent<PlayerAnimatorController>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackKeyKeyboard) || Input.GetKeyDown(attackKeyGamepad))
        {
            TryAttack();
            Debug.Log("Estoy atacando");
        }
    }

    private void TryAttack()
    {
        if (!CanAttack) return;
        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        if (playerAnimator != null)
            playerAnimator.PlayAttack();

        Vector3 attackDirection = playerMovement.LastMoveDirection;

        if (attackDirection.sqrMagnitude < 0.01f)
            attackDirection = Vector3.right;

        playerMovement.CanMove = false;

        yield return new WaitForSeconds(hitboxStartTime);

        if (swordHitbox != null)
            swordHitbox.EnableHitbox();

        yield return new WaitForSeconds(hitboxActiveTime);

        if (swordHitbox != null)
            swordHitbox.DisableHitbox();

        yield return new WaitForSeconds(attackMoveLockTime);

        playerMovement.CanMove = true;

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }
}