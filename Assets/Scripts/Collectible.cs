using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string itemName;
    public Sprite itemImage;
    public float popupDuration = 3f;

    private InventoryManager inventory;
    private PopupManager popupManager;

    [System.Obsolete]
    private void Start()
    {
        popupManager = FindObjectOfType<PopupManager>();
    }

    public void Interact(InventoryManager playerInventory)
    {
        if (playerInventory == null) return;

        playerInventory.AddItem(itemName);
        popupManager.ShowPopup(itemImage, popupDuration);
        gameObject.SetActive(false);
    }
}