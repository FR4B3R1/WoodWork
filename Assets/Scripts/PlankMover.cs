#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
#endif
using UnityEngine;

/// <summary>
/// Muove un oggetto lungo la Z DEL MONDO usando il delta verticale del mouse (<Pointer>/delta),
/// con fisica: Rigidbody NON kinematic + MovePosition in FixedUpdate.
/// - Normalizzazione per altezza schermo + integrazione con Time.unscaledDeltaTime (indipendente da FPS/timeScale).
/// - Limiti di corsa, smoothing esponenziale, sensibilità, max speed e turbo.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlankMover : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    [Header("Input (Mouse Delta)")]
    [Tooltip("InputActionReference a <Pointer>/delta (Action Type = Value, Control = Vector2, Pass Through consigliato).")]
    [SerializeField] private InputActionReference mouseDeltaAction;

    [Header("Corsa (lungo Z mondo)")]
    [Tooltip("Limiti dell'avanzamento (metri) rispetto alla posizione iniziale.")]
    [SerializeField] private Vector2 zLimits = new Vector2(-1.2f, 0.5f);

    [Header("Feeling")]
    [Tooltip("Metri/secondo percorsi se il mouse attraversa verticalmente TUTTO lo schermo.")]
    [SerializeField] private float rangePerScreen = 3.0f;      // veloce di base
    [Tooltip("Moltiplicatore di sensibilità (si applica sopra rangePerScreen).")]
    [SerializeField] private float sensitivity = 1.2f;         // aumenta la reattività
    [Tooltip("Velocità massima (m/s) dopo tutti i moltiplicatori. 0 = nessun limite.")]
    [SerializeField] private float maxSpeed = 4.5f;
    [Tooltip("Smorzamento esponenziale: più basso = più reattivo (es. 8-12).")]
    [SerializeField] private float smoothing = 10f;
    [Tooltip("Inverte il verso: se true, 'mouse su' diminuisce Z invece di aumentarla.")]
    [SerializeField] private bool invertDirection = false;

    [Header("Turbo (opzionale)")]
    [Tooltip("Tieni premuto per aumentare temporaneamente la sensibilità.")]
    [SerializeField] private KeyCode turboKey = KeyCode.LeftShift;
    [SerializeField] private float turboMultiplier = 1.8f;

    [Header("Rigidbody (consigliato)")]
    [Tooltip("Blocca X/Y e TUTTE le rotazioni per scorrere solo lungo Z.")]
    [SerializeField] private bool freezeOtherAxes = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    // Stato
    private Rigidbody rb;
    private Vector3 basePoint;         // posizione d'origine (X/Y/Z iniziali)
    private float zCurrent;            // offset lungo Z rispetto a basePoint
    private float zTarget;             // target filtrato
    private float lastUnitsPerSecond;  // per debug

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false; // MovePosition richiede NON kinematic
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        if (freezeOtherAxes)
        {
            rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY
                           | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
        }

        basePoint = transform.position;
        zCurrent = transform.position.z - basePoint.z;
        zTarget = Mathf.Clamp(zCurrent, zLimits.x, zLimits.y);

        if (Mathf.Approximately(zLimits.x, zLimits.y))
            Debug.LogWarning("[PlankMover_Z_Physics] zLimits min==max: nessuna corsa disponibile.");
    }

    private void OnEnable()
    {
        if (mouseDeltaAction && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();
        else
            Debug.LogWarning("[PlankMover_Z_Physics] mouseDeltaAction non assegnata o action null.");
    }

    private void OnDisable()
    {
        if (mouseDeltaAction && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Disable();
    }

    private void Update()
    {
        if (mouseDeltaAction == null || mouseDeltaAction.action == null) return;

        // 1) Leggi delta mouse (pixel/frame)
        Vector2 d = mouseDeltaAction.action.ReadValue<Vector2>();
        float dY = d.y;

        // 2) Normalizza per altezza schermo -> frazione schermo per frame
        float frac = dY / Mathf.Max(1f, (float)Screen.height);

        // 3) Calcola velocità in m/s
        float sign = invertDirection ? -1f : 1f;
        float turbo = (turboMultiplier > 0f && Input.GetKey(turboKey)) ? turboMultiplier : 1f;
        float unitsPerSecond = frac * rangePerScreen * sensitivity * turbo * sign;

        // 4) Clamp velocità massima
        if (maxSpeed > 0f)
            unitsPerSecond = Mathf.Clamp(unitsPerSecond, -maxSpeed, maxSpeed);

        lastUnitsPerSecond = unitsPerSecond;

        // 5) Integrazione (indipendente da FPS/timeScale)
        zTarget = Mathf.Clamp(zTarget + unitsPerSecond * Time.unscaledDeltaTime, zLimits.x, zLimits.y);

        // 6) Smoothing esponenziale per zCurrent
        float alpha = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        zCurrent = Mathf.Lerp(zCurrent, zTarget, alpha);

        if (logDebug && Time.frameCount % 20 == 0)
        {
            Debug.Log($"[PlankMover_Z_Physics] dY={dY:F1}, frac={frac:F4}, speed={unitsPerSecond:F2} m/s, z={zCurrent:F3}->{zTarget:F3}");
        }
    }

    private void FixedUpdate()
    {
        // Applica lo spostamento solo su Z; X/Y restano quelle di basePoint
        Vector3 desired = new Vector3(basePoint.x, basePoint.y, basePoint.z + zCurrent);
        rb.MovePosition(desired);
    }

    /// <summary>Rimposta la base all'attuale posizione (se riposizioni il pezzo in runtime).</summary>
    public void RebaseHere()
    {
        basePoint = transform.position;
        zCurrent = 0f;
        zTarget = Mathf.Clamp(0f, zLimits.x, zLimits.y);
    }
#endif
}
