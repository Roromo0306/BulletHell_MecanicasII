using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 6f;
    public float acceleration = 18f;
    public float deceleration = 22f;
    public float deadZone = 0.25f;

    [Header("References")]
    public Transform visual;
    public Transform swordPivot;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;

    private PlayerHealth playerHealth;

    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 currentVelocity;

    private Vector3 originalVisualScale;
    private Vector3 originalSwordPivotScale;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.left;
    public bool CanMove { get; set; } = true;

    // Tu arte base mira a la izquierda
    public int FacingDirection { get; private set; } = -1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (visual != null)
            originalVisualScale = visual.localScale;

        if (swordPivot != null)
            originalSwordPivotScale = swordPivot.localScale;

        playerHealth = GetComponent<PlayerHealth>();

        FlipCharacter();
    }

    private void Update()
    {
        ReadInput();
        FlipCharacter();

        if (Input.GetKeyDown(KeyCode.JoystickButton3) || Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Botón dash pulsado");

            if (canDash && CanMove)
            {
                StartCoroutine(Dash());
            }
        }
    }

    private void FixedUpdate()
    {
        Move();
    }

    private void ReadInput()
    {
        if (!CanMove)
        {
            moveInput = Vector3.zero;
            return;
        }

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        if (Mathf.Abs(horizontal) < deadZone)
            horizontal = 0f;

        if (Mathf.Abs(vertical) < deadZone)
            vertical = 0f;

        moveInput = new Vector3(horizontal, 0f, vertical);

        if (moveInput.sqrMagnitude > 1f)
            moveInput.Normalize();

        if (moveInput.sqrMagnitude > 0.01f)
            LastMoveDirection = moveInput.normalized;
    }

    private void Move()
    {
        if (!isDashing)
        {
            Vector3 targetVelocity = moveInput * moveSpeed;

            float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;

            currentVelocity = Vector3.MoveTowards(
                currentVelocity,
                targetVelocity,
                rate * Time.fixedDeltaTime
            );

            rb.velocity = new Vector3(
                currentVelocity.x,
                rb.velocity.y,
                currentVelocity.z
            );
        }
    }

    private void FlipCharacter()
    {
        if (isDashing)
            return;

        if (moveInput.x > 0.05f)
            FacingDirection = 1;
        else if (moveInput.x < -0.05f)
            FacingDirection = -1;

        bool lookingRight = FacingDirection == 1;

        if (visual != null)
        {
            float xScale = Mathf.Abs(originalVisualScale.x);

            // Arte base del player mira a la IZQUIERDA:
            // izquierda = positivo
            // derecha = negativo
            visual.localScale = new Vector3(
                lookingRight ? -xScale : xScale,
                originalVisualScale.y,
                originalVisualScale.z
            );
        }

        if (swordPivot != null)
        {
            float xScale = Mathf.Abs(originalSwordPivotScale.x);

            // Arte base de la espada también está a la IZQUIERDA:
            // izquierda = positivo
            // derecha = negativo
            swordPivot.localScale = new Vector3(
                lookingRight ? xScale : -xScale,
                originalSwordPivotScale.y,
                originalSwordPivotScale.z
            );
        }
    }

    IEnumerator Dash()
    {
        isDashing = true;
        canDash = false;

        Debug.Log("Dash iniciado");

        Vector3 dashDirection = moveInput.sqrMagnitude > 0.01f
            ? moveInput.normalized
            : LastMoveDirection.normalized;

        if (playerHealth != null)
            playerHealth.StartInvulnerability(dashDuration);

        float timer = 0f;

        while (timer < dashDuration)
        {
            rb.velocity = new Vector3(
                dashDirection.x * dashSpeed,
                rb.velocity.y,
                dashDirection.z * dashSpeed
            );

            timer += Time.deltaTime;
            yield return null;
        }

        currentVelocity = dashDirection * moveSpeed;
        isDashing = false;

        Debug.Log("Dash terminado");

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}