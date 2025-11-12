using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public bool hasHelmet = false;
    public bool hasGloves = false;
    public bool hasGlasses = false;
    public bool hasCuffie = false;

    public bool helmetEquipped = false;
    public bool glovesEquipped = false;
    public bool glassesEquipped = false;
    public bool cuffieEquipped = false;

    public bool allEquipped = false;    

    public void AddItem(string itemName)
    {
        switch (itemName)
        {
            case "Casco":
                hasHelmet = true;
                helmetEquipped = true; // equip automatico

                break;
            case "Guanti":
                hasGloves = true;
                glovesEquipped = true;

                break;
            case "Occhiali":
                hasGlasses = true;
                glassesEquipped = true;

                break;
                case "Cuffie":
                hasCuffie = true;
                cuffieEquipped = true;
                break;

        }

        if (hasHelmet && hasGloves && hasGlasses && hasCuffie == true)
        {
            allEquipped = true;
        }


    }
}