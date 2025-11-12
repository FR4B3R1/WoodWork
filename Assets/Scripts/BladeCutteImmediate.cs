using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class BladeCutterImmediate : MonoBehaviour
{
    [Header("Filtro oggetti da tagliare")]
    [SerializeField] private string plankTag = "Plank";

    [Header("Prefab metà")]
    [SerializeField] private GameObject halfLeftPrefab;
    [SerializeField] private GameObject halfRightPrefab;

    [Header("Direzioni/offset")]
    [Tooltip("Transform di riferimento della lama. right = separazione laterale, forward = spinta in uscita.")]
    [SerializeField] private Transform bladeRef;

    [Tooltip("Distanza iniziale tra le due metà quando appaiono.")]
    [SerializeField] private float halvesOffset = 0.01f;

    [Header("Impulsi fisici")]
    [SerializeField] private float sideImpulse = 1.5f;
    [SerializeField] private float forwardKick = 0.4f;
    [Tooltip("Se > 0, abilita la gravità sulle metà dopo N secondi (utile per un effetto più 'pulito').")]
    [SerializeField] private float gravityDelay = 0.0f;

    [Header("Audio accensione")]
    [SerializeField] private AudioSource audioSource; // Solo per accensione
    [SerializeField] private AudioClip powerOnSfx;

    [Header("Auto cleanup (opzionale)")]
    [Tooltip("Se > 0, distrugge automaticamente le metà dopo N secondi.")]
    [SerializeField] private float destroyHalvesAfter = 0f;

    [Header("Eventi")]
    public UnityEvent OnCut;

    private Collider triggerCol;
    private bool machineOn = false;

    private void Awake()
    {
        machineOn = false; // Forza spento all'inizio
        triggerCol = GetComponent<Collider>();
        if (!triggerCol.isTrigger)
        {
            Debug.LogWarning("[BladeCutterImmediate] Il collider della lama deve avere IsTrigger = ON.");
        }

        if (!bladeRef)
        {
            Debug.LogWarning("[BladeCutterImmediate] 'bladeRef' non assegnato: userò assi del mondo (right/forward).");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            machineOn = !machineOn;
            Debug.Log("[BladeCutterImmediate] Macchinario " + (machineOn ? "ACCESO" : "SPENTO"));

            if (audioSource)
            {
                if (machineOn && powerOnSfx)
                {
                    audioSource.clip = powerOnSfx;
                    audioSource.Play();
                }
                else
                {
                    audioSource.Stop(); // Interrompe subito il suono
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!machineOn) return;

        Transform plankRoot = GetPlankRoot(other.transform);
        if (plankRoot == null || !plankRoot.CompareTag(plankTag))
        {
            return;
        }

        DoSplit(plankRoot);
    }

    private Transform GetPlankRoot(Transform tr)
    {
        Transform cur = tr;
        int guard = 32;
        while (cur != null && guard-- > 0)
        {
            if (cur.CompareTag(plankTag))
                return cur;
            cur = cur.parent;
        }
        return null;
    }

    private void DoSplit(Transform whole)
    {
        if (!halfLeftPrefab || !halfRightPrefab)
        {
            Debug.LogError("[BladeCutterImmediate] Prefab delle metà non assegnati.");
            return;
        }

        Vector3 side = bladeRef ? bladeRef.right : Vector3.right;
        Vector3 fwd = bladeRef ? bladeRef.forward : Vector3.forward;

        Vector3 pos = whole.position;
        Quaternion rot = whole.rotation;

        whole.gameObject.SetActive(false);

        GameObject left = Instantiate(halfLeftPrefab, pos - side * halvesOffset, rot);
        GameObject right = Instantiate(halfRightPrefab, pos + side * halvesOffset, rot);

        if (left.TryGetComponent<Rigidbody>(out var rbL))
        {
            rbL.isKinematic = false;
            if (gravityDelay > 0f) rbL.useGravity = false;
            rbL.AddForce((-side * sideImpulse) + (fwd * forwardKick), ForceMode.Impulse);
            if (gravityDelay > 0f)
            {
                var eg = left.AddComponent<EnableGravityAfter>();
                eg.Delay = gravityDelay;
            }
        }

        if (right.TryGetComponent<Rigidbody>(out var rbR))
        {
            rbR.isKinematic = false;
            if (gravityDelay > 0f) rbR.useGravity = false;
            rbR.AddForce((side * sideImpulse) + (fwd * forwardKick), ForceMode.Impulse);
            if (gravityDelay > 0f)
            {
                var eg = right.AddComponent<EnableGravityAfter>();
                eg.Delay = gravityDelay;
            }
        }

        OnCut?.Invoke();

        if (destroyHalvesAfter > 0f)
        {
            Destroy(left, destroyHalvesAfter);
            Destroy(right, destroyHalvesAfter);
        }
    }

    private class EnableGravityAfter : MonoBehaviour
    {
        public float Delay = 0.2f;
        private float t;

        private void Update()
        {
            t += Time.deltaTime;
            if (t >= Delay && TryGetComponent<Rigidbody>(out var rb))
            {
                rb.useGravity = true;
                Destroy(this);
            }
        }
    }
}