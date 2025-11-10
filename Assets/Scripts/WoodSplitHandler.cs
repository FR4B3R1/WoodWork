using UnityEngine;

public class WoodSplitHandler : MonoBehaviour
{
    [Header("Riferimenti legno")]
    [SerializeField] private GameObject intactWood;   // Legno intero
    [SerializeField] private GameObject leftHalf;     // Prima metà
    [SerializeField] private GameObject rightHalf;    // Seconda metà

    [Header("Fisica")]
    [Tooltip("Impulso applicato alle due metà al momento del taglio")]
    [SerializeField] private float splitImpulse = 2f;
    [Tooltip("Direzione di separazione (es. Vector3.right per separare lungo X)")]
    [SerializeField] private Vector3 separationDirection = Vector3.right;

    [Header("Effetti (facoltativi)")]
    [SerializeField] private ParticleSystem dustFX;
    [SerializeField] private AudioSource cutSFX;

    public void PerformCut()
    {
        // Disattiva il legno intero
        if (intactWood != null) intactWood.SetActive(false);

        // Attiva le due metà
        if (leftHalf != null) leftHalf.SetActive(true);
        if (rightHalf != null) rightHalf.SetActive(true);

        // Applica forza alle due metà
        ApplyImpulse(leftHalf, -1f);
        ApplyImpulse(rightHalf, +1f);

        // Effetti
        if (dustFX != null) dustFX.Play();
        if (cutSFX != null) cutSFX.Play();
    }

    private void ApplyImpulse(GameObject half, float dirSign)
    {
        if (half == null) return;
        Rigidbody rb = half.GetComponent<Rigidbody>();
        if (rb == null) return;

        rb.isKinematic = false; // Assicurati che sia disattivato per la fisica
        Vector3 worldDir = separationDirection.normalized * dirSign;
        rb.AddForce(worldDir * splitImpulse, ForceMode.Impulse);
    }
}