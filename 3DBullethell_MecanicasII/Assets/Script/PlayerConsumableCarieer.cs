using UnityEngine;

public class PlayerConsumableCarrier : MonoBehaviour
{
    [Header("References")]
    public PlayerMovement playerMovement;
    public PlayerHealth playerHealth;
    public PlayerCarryObject playerCarryObject;
    public Transform consumableHoldPoint;
    public TimeSlowManager timeSlowManager;

    [Header("Pickup")]
    public KeyCode pickupKeyKeyboard = KeyCode.F;
    public KeyCode pickupKeyGamepad = KeyCode.JoystickButton2;
    public float pickupRange = 1.3f;
    public LayerMask consumableLayer;

    [Header("Use")]
    public KeyCode useKeyKeyboard = KeyCode.Q;
    public KeyCode useKeyGamepad = KeyCode.JoystickButton6; // L2 normalmente

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
        HandlePickup();
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

    private void HandlePickup()
    {
        
        if (Input.GetKeyDown(pickupKeyKeyboard) || Input.GetKeyDown(pickupKeyGamepad))
        {
            
            if (HasConsumable)
            {
                DropConsumable();
                return;
            }

           
            if (nearbyConsumable != null)
            {
                if (playerCarryObject != null && playerCarryObject.HasObject)
                    return;

                PickUp(nearbyConsumable);
            }
        }
    }

    private void PickUp(ConsumableObject consumable)
    {
        heldConsumable = consumable;
        heldConsumable.PickUp(consumableHoldPoint);
    }

    private void HandleUse()
    {
        if (!HasConsumable) return;

        if (Input.GetKeyDown(useKeyKeyboard) || Input.GetKeyDown(useKeyGamepad))
        {
            UseConsumable();
        }
    }

    private void UseConsumable()
    {
        if (heldConsumable == null) return;

        switch (heldConsumable.consumableType)
        {
            case ConsumableType.Heart:
                UseHeart();
                break;

            case ConsumableType.Hourglass:
                UseHourglass();
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

        ConsumeHeld();
    }

    private void ConsumeHeld()
    {
        heldConsumable.Consume();
        heldConsumable = null;
    }

    private void UpdateHeldFlip()
    {
        if (!HasConsumable) return;
        if (playerMovement == null) return;

        heldConsumable.SetFacing(playerMovement.FacingDirection);
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
    }
}