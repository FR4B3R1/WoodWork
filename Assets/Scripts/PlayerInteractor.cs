using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    public InputActionReference interactAction;
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    private Camera playerCamera;
    private InventoryManager inventory;

    private void Awake()
    {
        playerCamera = Camera.main;
        inventory = GetComponent<InventoryManager>();
    }

    private void OnEnable()
    {
        interactAction.action.Enable();
        interactAction.action.performed += OnInteract;
    }

    private void OnDisable()
    {
        interactAction.action.performed -= OnInteract;
        interactAction.action.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            Collectible collectible = hit.collider.GetComponent<Collectible>();
            if (collectible != null)
            {
                collectible.Interact(inventory);
            }
        }
    }
}
