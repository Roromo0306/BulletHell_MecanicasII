using UnityEngine;

public class PlayerCarryObject : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerAttack playerAttack;
    public Transform holdPoint;
    public GameObject swordVisual;

    [Header("Pickup")]
    public KeyCode pickupKeyKeyboard = KeyCode.F;
    public KeyCode pickupKeyGamepad = KeyCode.JoystickButton1;
    public float pickupRange = 1.3f;
    public LayerMask carryObjectLayer;

    [Header("Throw Input")]
    public KeyCode throwKeyKeyboard = KeyCode.E;
    public KeyCode throwKeyGamepad = KeyCode.JoystickButton7; // R2 normalmente

    [Header("Aim Visuals")]
    public LineRenderer aimLine;
    public ChargeFillUI chargeFillUI;

    private CarryThrowableObject heldObject;
    private CarryThrowableObject nearbyObject;
    private float chargeTimer;
    private bool isCharging;

    public bool HasObject => heldObject != null;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerAttack == null)
            playerAttack = GetComponent<PlayerAttack>();

        HideAimVisuals();
    }

    private void Update()
    {
        DetectNearbyObject();
        HandlePickup();
        HandleThrowCharge();
        UpdateHeldObjectFlip();
    }

    private void DetectNearbyObject()
    {
        if (HasObject)
        {
            nearbyObject = null;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, carryObjectLayer);

        nearbyObject = null;

        foreach (Collider hit in hits)
        {
            CarryThrowableObject obj = hit.GetComponentInParent<CarryThrowableObject>();

            if (obj != null && obj.CanBePickedUp)
            {
                nearbyObject = obj;
                return;
            }
        }
    }

    private void HandlePickup()
    {
        if (HasObject) return;
        if (nearbyObject == null) return;

        if (Input.GetKeyDown(pickupKeyKeyboard) || Input.GetKeyDown(pickupKeyGamepad))
        {
            PickUp(nearbyObject);
        }
    }

    private void PickUp(CarryThrowableObject obj)
    {
        heldObject = obj;
        heldObject.PickUp(holdPoint);

        if (swordVisual != null)
            swordVisual.SetActive(false);

        if (playerAttack != null)
            playerAttack.CanAttack = false;
    }

    private void HandleThrowCharge()
    {
        if (!HasObject) return;

        bool throwHeld = Input.GetKey(throwKeyKeyboard) || Input.GetKey(throwKeyGamepad);
        bool throwReleased = Input.GetKeyUp(throwKeyKeyboard) || Input.GetKeyUp(throwKeyGamepad);

        if (throwHeld)
        {
            isCharging = true;
            chargeTimer += Time.deltaTime;

            float charge01 = heldObject.GetCharge01(chargeTimer);

            UpdateAimVisuals(charge01);
        }

        if (throwReleased && isCharging)
        {
            ThrowHeldObject();
        }
    }

    private void ThrowHeldObject()
    {
        float charge01 = heldObject.GetCharge01(chargeTimer);

        Vector3 direction = playerMovement.LastMoveDirection;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.right * playerMovement.FacingDirection;

        heldObject.Throw(direction.normalized, charge01);

        heldObject = null;
        chargeTimer = 0f;
        isCharging = false;

        HideAimVisuals();

        if (swordVisual != null)
            swordVisual.SetActive(true);

        if (playerAttack != null)
            playerAttack.CanAttack = true;
    }

    private void UpdateHeldObjectFlip()
    {
        if (!HasObject) return;

        heldObject.SetFacing(playerMovement.FacingDirection);
    }

    private void UpdateAimVisuals(float charge01)
    {
        if (heldObject == null) return;

        Vector3 direction = playerMovement.LastMoveDirection;

        if (direction.sqrMagnitude < 0.01f)
            direction = Vector3.right * playerMovement.FacingDirection;

        direction.y = 0f;
        direction.Normalize();

        float distance = heldObject.GetThrowDistance(charge01);

        Vector3 start = holdPoint.position + Vector3.up * 0.15f;
        Vector3 end = start + direction * distance;

        if (aimLine != null)
        {
            aimLine.gameObject.SetActive(true);
            aimLine.positionCount = 2;
            aimLine.SetPosition(0, start);
            aimLine.SetPosition(1, end);
        }

        if (chargeFillUI != null)
        {
            chargeFillUI.Show();
            chargeFillUI.SetFill(charge01);
        }
    }

    private void HideAimVisuals()
    {
        if (aimLine != null)
            aimLine.gameObject.SetActive(false);

        if (chargeFillUI != null)
            chargeFillUI.Hide();
    }
}