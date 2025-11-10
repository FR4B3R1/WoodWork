using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public Transform holdPoint;  // Un empty object davanti alla camera
    public float pickupRange = 3f;
    private GameObject heldObject;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObject == null)
                TryPickup();
            else
                Drop();
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
                heldObject.transform.position = holdPoint.position;
                heldObject.transform.SetParent(holdPoint);
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