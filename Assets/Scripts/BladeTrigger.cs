using UnityEngine;
using EzySlice;

public class BladeTrigger : MonoBehaviour
{
    public Material cutMaterial;
    public Transform blade; // La lama (può essere il parent)
    private GameObject woodPieceInContact;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wood"))
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

    void SliceWood(GameObject woodPiece)
    {
        SlicedHull hull = woodPiece.Slice(blade.position, blade.up, cutMaterial);

        if (hull != null)
        {
            GameObject upperHull = hull.CreateUpperHull(woodPiece, cutMaterial);
            GameObject lowerHull = hull.CreateLowerHull(woodPiece, cutMaterial);

            AddHullComponents(upperHull);
            AddHullComponents(lowerHull);

            Destroy(woodPiece);
        }
    }

    void AddHullComponents(GameObject obj)
    {
        obj.AddComponent<MeshCollider>().convex = true;
        obj.AddComponent<Rigidbody>();
    }
}
