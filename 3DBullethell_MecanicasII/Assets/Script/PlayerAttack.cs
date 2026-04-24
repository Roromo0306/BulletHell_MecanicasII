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
    public float attackCooldown = 0.35f;
    public float attackMoveLockTime = 0.18f;

    [Header("References")]
    public PlayerMovement playerMovement;
    public Transform swordPivot;
    public SwordHitbox swordHitbox;

    private bool isAttacking;
    private float nextAttackTime;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(attackKeyKeyboard) || Input.GetKeyDown(attackKeyGamepad))
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        if (isAttacking) return;
        if (Time.time < nextAttackTime) return;

        StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackCooldown;

        Vector3 attackDirection = playerMovement.LastMoveDirection;

        if (attackDirection.sqrMagnitude < 0.01f)
            attackDirection = Vector3.right;

        //PositionSwordHitbox(attackDirection);
        playerMovement.CanMove = false;

        yield return new WaitForSeconds(hitboxStartTime);

        swordHitbox.EnableHitbox();

        yield return new WaitForSeconds(hitboxActiveTime);

        swordHitbox.DisableHitbox();

        yield return new WaitForSeconds(attackMoveLockTime);

        playerMovement.CanMove = true;

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
    }


   
}