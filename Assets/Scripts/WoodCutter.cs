using UnityEngine;
using EzySlice;

public class WoodCutter : MonoBehaviour
{
    public Material cutMaterial;
    public Transform blade;
    private GameObject woodPieceInContact;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wood")) // Assicurati che il cubo abbia il tag "Wood"
        {
            woodPieceInContact = other.gameObject;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Wood"))
        {
            woodPieceInContact = null;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && woodPieceInContact != null)
        {
            SliceWood(woodPieceInContact);
        }
    }

    [System.Obsolete]
    void SliceWood(GameObject woodPiece)
    {
        SlicedHull hull = woodPiece.Slice(blade.position, blade.up, cutMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(woodPiece, cutMaterial);
            GameObject lowerHull = hull.CreateLowerHull(woodPiece, cutMaterial);

            upperHull.transform.position += blade.up * 0.01f;
            lowerHull.transform.position -= blade.up * 0.01f;

            AddHullComponents(upperHull);
            AddHullComponents(lowerHull);


            Destroy(woodPiece);
        }
    }

    [System.Obsolete]
    void AddHullComponents(GameObject obj)
    {
        MeshCollider collider = obj.AddComponent<MeshCollider>();
        collider.convex = true;

        Rigidbody rb = obj.AddComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        rb.mass = 2f; // aumenta la massa per ridurre il "volo"
        rb.useGravity = true;
        rb.drag = 1f; // aggiungi resistenza all'aria
        rb.angularDrag = 2f; // riduce rotazioni eccessive
    }
}