using UnityEngine;
using System.Collections.Generic;

public class PickupZone : MonoBehaviour
{
    public Transform holdPoint;
    public Vector3 rotationOffset = new Vector3(0f, 90f, 0f);

    private GameObject heldObject;
    private List<GameObject> nearbyObjects = new List<GameObject>();

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            nearbyObjects.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Pickup"))
        {
            nearbyObjects.Remove(other.gameObject);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && heldObject == null && nearbyObjects.Count > 0)
        {
            // Prendi il primo oggetto nella lista
            heldObject = nearbyObjects[0];
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            rb.isKinematic = true;

            heldObject.transform.SetParent(holdPoint);
            heldObject.transform.localPosition = Vector3.zero;
            heldObject.transform.localRotation = Quaternion.Euler(rotationOffset);
        }
    }
}

