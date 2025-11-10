using UnityEngine;

public class SawCut : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Cuttable"))
        {
            SplitObject(collision.gameObject);
        }
    }

    void SplitObject(GameObject target)
    {
        Vector3 pos = target.transform.position;
        Vector3 scale = target.transform.localScale;

        // Creiamo due metà
        GameObject half1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        GameObject half2 = GameObject.CreatePrimitive(PrimitiveType.Cube);

        half1.transform.position = pos + new Vector3(0, 0, -scale.z / 4);
        half2.transform.position = pos + new Vector3(0, 0, scale.z / 4);

        half1.transform.localScale = new Vector3(scale.x, scale.y, scale.z / 2);
        half2.transform.localScale = new Vector3(scale.x, scale.y, scale.z / 2);

        half1.GetComponent<Renderer>().material = target.GetComponent<Renderer>().material;
        half2.GetComponent<Renderer>().material = target.GetComponent<Renderer>().material;

        half1.AddComponent<Rigidbody>();
        half2.AddComponent<Rigidbody>();

        Destroy(target);
    }
}
