using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public bool hasHelmet = false;
    public bool hasGloves = false;
    public bool hasGlasses = false;

    public bool helmetEquipped = false;
    public bool glovesEquipped = false;
    public bool glassesEquipped = false;

    public bool allEquipped = false;    

    public void AddItem(string itemName)
    {
        switch (itemName)
        {
            case "Helmet":
                hasHelmet = true;
                helmetEquipped = true; // equip automatico

                break;
            case "Gloves":
                hasGloves = true;
                glovesEquipped = true;

                break;
            case "Glasses":
                hasGlasses = true;
                glassesEquipped = true;

                break;
        }

        allEquipped = helmetEquipped && glovesEquipped && glassesEquipped;
    }
}