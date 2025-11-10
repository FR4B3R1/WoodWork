using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public Transform holdPoint;  // punto davanti alla camera dove tenere gli oggetti
    public float pickupRange = 3f;
    private GameObject heldObject;

    // Offset e rotazione predefiniti per la sega
    public Vector3 holdOffset = new Vector3(0f, -0.2f, 0.4f);
    public Vector3 holdRotation = new Vector3(0f, 0f, 90f);

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickup();
            else
                Drop();
        }

        // Mantiene la sega "ancorata" correttamente anche durante il movimento
        if (heldObject != null)
        {
            heldObject.transform.position = holdPoint.TransformPoint(holdOffset);
            heldObject.transform.rotation = holdPoint.rotation * Quaternion.Euler(0f,90f,0f);
        }
    }

    void TryPickup()
    {
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange))
        {
            if (hit.collider.CompareTag("Pickup"))
            {
                heldObject = hit.collider.gameObject;
                heldObject.GetComponent<Rigidbody>().isKinematic = true;
                heldObject.transform.SetParent(holdPoint);

                // Imposta subito posizione e rotazione
                heldObject.transform.localPosition = holdOffset;
                //heldObject.transform.localRotation = Quaternion.Euler(holdOffset);
            }
        }
    }

    void Drop()
    {
        heldObject.GetComponent<Rigidbody>().isKinematic = false;
        heldObject.transform.SetParent(null);
        heldObject = null;
    }
}