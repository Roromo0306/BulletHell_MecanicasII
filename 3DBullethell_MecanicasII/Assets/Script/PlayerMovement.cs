using UnityEngine;

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


    private Rigidbody rb;
    private Vector3 moveInput;
    private Vector3 currentVelocity;

    private Vector3 originalVisualScale;
    private Vector3 originalSwordPivotScale;

    public Vector3 LastMoveDirection { get; private set; } = Vector3.right;
    public bool CanMove { get; set; } = true;
    public int FacingDirection { get; private set; } = 1;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (visual != null)
            originalVisualScale = visual.localScale;

        if (swordPivot != null)
            originalSwordPivotScale = swordPivot.localScale;
    }

    private void Update()
    {
        ReadInput();
        FlipCharacter();
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

    private void FlipCharacter()
    {
        if (moveInput.x > 0.05f)
            FacingDirection = 1;
        else if (moveInput.x < -0.05f)
            FacingDirection = -1;

        if (visual != null)
        {
            visual.localScale = new Vector3(
                Mathf.Abs(originalVisualScale.x) * FacingDirection,
                originalVisualScale.y,
                originalVisualScale.z
            );
        }

        if (swordPivot != null)
        {
            swordPivot.localScale = new Vector3(
                Mathf.Abs(originalSwordPivotScale.x) * FacingDirection,
                originalSwordPivotScale.y,
                originalSwordPivotScale.z
            );
        }
    }
}