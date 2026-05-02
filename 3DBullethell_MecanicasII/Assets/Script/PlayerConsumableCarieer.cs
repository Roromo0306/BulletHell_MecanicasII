using UnityEngine;

public class PlayerConsumableCarrier : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;
    public PlayerCarryObject playerCarryObject;
    public Transform consumableHoldPoint;
    public TimeSlowManager timeSlowManager;
    public BubbleShieldController bubbleShieldController;

    [Header("Pickup / Drop")]
    public KeyCode pickupKeyKeyboard = KeyCode.F;
    public KeyCode pickupKeyGamepad = KeyCode.JoystickButton2;
    public float pickupRange = 1.3f;
    public LayerMask consumableLayer;

    [Header("Use")]
    public KeyCode useKeyKeyboard = KeyCode.Q;
    public KeyCode useKeyGamepad = KeyCode.JoystickButton6; // prueba también 7, 4, 5

    [Header("Optional L2 Axis")]
    public bool useAxisForL2 = false;
    public string l2AxisName = "L2";
    public float l2AxisThreshold = 0.5f;

    private bool l2AxisWasHeld;

    private ConsumableObject heldConsumable;
    private ConsumableObject nearbyConsumable;

    public bool HasConsumable => heldConsumable != null;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();

        if (playerCarryObject == null)
            playerCarryObject = GetComponent<PlayerCarryObject>();
    }

    private void Update()
    {
        DetectNearbyConsumable();
        HandlePickupOrDrop();
        HandleUse();
        UpdateHeldFlip();
    }

    private void DetectNearbyConsumable()
    {
        if (HasConsumable)
        {
            nearbyConsumable = null;
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, pickupRange, consumableLayer);

        nearbyConsumable = null;

        foreach (Collider hit in hits)
        {
            ConsumableObject consumable = hit.GetComponentInParent<ConsumableObject>();

            if (consumable != null && consumable.CanBePickedUp)
            {
                nearbyConsumable = consumable;
                return;
            }
        }
    }

    private void HandlePickupOrDrop()
    {
        if (!Input.GetKeyDown(pickupKeyKeyboard) && !Input.GetKeyDown(pickupKeyGamepad))
            return;

        if (HasConsumable)
        {
            DropConsumable();
            return;
        }

        if (nearbyConsumable == null) return;

        if (playerCarryObject != null && playerCarryObject.HasObject)
            return;

        PickUp(nearbyConsumable);
    }

    private void PickUp(ConsumableObject consumable)
    {
        heldConsumable = consumable;
        heldConsumable.PickUp(consumableHoldPoint);

        Debug.Log("Consumible recogido: " + heldConsumable.consumableType);
    }

    private void DropConsumable()
    {
        if (heldConsumable == null) return;

        Vector3 dropDir = playerMovement.LastMoveDirection;

        if (dropDir.sqrMagnitude < 0.01f)
            dropDir = Vector3.right * playerMovement.FacingDirection;

        dropDir.y = 0f;
        dropDir.Normalize();

        Vector3 dropPos = transform.position + dropDir * 0.8f;
        dropPos.y = transform.position.y;

        heldConsumable.Drop(dropPos);
        heldConsumable = null;

        Debug.Log("Consumible soltado");
    }

    private void HandleUse()
    {
        if (!HasConsumable) return;

        if (UseButtonPressed())
        {
            Debug.Log("Botón usar consumible pulsado");
            UseConsumable();
        }
    }

    private bool UseButtonPressed()
    {
        bool pressed = Input.GetKeyDown(useKeyKeyboard) || Input.GetKeyDown(useKeyGamepad);

        if (useAxisForL2)
        {
            float axisValue = 0f;

            try
            {
                axisValue = Mathf.Abs(Input.GetAxisRaw(l2AxisName));
            }
            catch
            {
                axisValue = 0f;
            }

            bool axisHeld = axisValue > l2AxisThreshold;

            if (axisHeld && !l2AxisWasHeld)
                pressed = true;

            l2AxisWasHeld = axisHeld;
        }

        return pressed;
    }

    private void UseConsumable()
    {
        if (heldConsumable == null) return;

        Debug.Log("Usando consumible: " + heldConsumable.consumableType);

        switch (heldConsumable.consumableType)
        {
            case ConsumableType.Heart:
                UseHeart();
                break;

            case ConsumableType.Hourglass:
                UseHourglass();
                break;

            case ConsumableType.BubbleShield:
                UseBubbleShield();
                break;
        }
    }

    private void UseHeart()
    {
        if (playerHealth == null) return;

        if (playerHealth.currentHearts >= playerHealth.maxHearts)
        {
            Debug.Log("Vida llena. No se consume el corazón.");
            return;
        }

        playerHealth.Heal(1);
        ConsumeHeld();
    }

    private void UseHourglass()
    {
        if (timeSlowManager != null)
            timeSlowManager.PlaySlowMotion();
        else
            Debug.LogWarning("Falta asignar TimeSlowManager");

        ConsumeHeld();
    }

    private void UseBubbleShield()
    {
        if (bubbleShieldController != null)
        {
            Debug.Log("Activando Bubble Shield");
            bubbleShieldController.Activate();
            ConsumeHeld();
        }
        else
        {
            Debug.LogWarning("Falta asignar BubbleShieldController en PlayerConsumableCarrier");
        }
    }

    private void ConsumeHeld()
    {
        if (heldConsumable == null) return;

        heldConsumable.Consume();
        heldConsumable = null;
    }

    private void UpdateHeldFlip()
    {
        if (!HasConsumable) return;
        if (playerMovement == null) return;

        heldConsumable.SetFacing(playerMovement.FacingDirection);
    }
}