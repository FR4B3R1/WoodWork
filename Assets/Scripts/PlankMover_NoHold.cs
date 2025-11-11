#if ENABLE_INPUT_SYSTEM
using UnityEngine;
using UnityEngine.InputSystem;
#endif
using UnityEngine;

/// <summary>
/// Muove l'oggetto lungo la Z DEL MONDO usando il delta verticale del mouse (<Pointer>/delta),
/// senza fisica (usa transform.position).
/// Nessun hold: appena lo script è abilitato, l'input guida il movimento.
/// - Normalizzazione per altezza schermo + integrazione con Time.unscaledDeltaTime (indipendente da FPS/timeScale).
/// - Limiti di corsa e smoothing esponenziale.
/// </summary>
public class PlankMover_Z_NoHold : MonoBehaviour
{
#if ENABLE_INPUT_SYSTEM
    [Header("Input (Mouse Delta)")]
    [Tooltip("InputActionReference a <Pointer>/delta (Action Type=Value, Control=Vector2, Pass Through consigliato).")]
    [SerializeField] private InputActionReference mouseDeltaAction;

    [Header("Corsa (lungo Z mondo)")]
    [Tooltip("Limiti dell'avanzamento (metri) rispetto alla posizione iniziale.")]
    [SerializeField] private Vector2 zLimits = new Vector2(-1.2f, 0.4f);

    [Header("Feeling")]
    [Tooltip("Metri/secondo se il mouse attraversa verticalmente TUTTO lo schermo.")]
    [SerializeField] private float rangePerScreen = 1.0f;
    [Tooltip("Smorzamento esponenziale (12–18 = reattivo ma morbido).")]
    [SerializeField] private float smoothing = 16f;
    [Tooltip("Se true, 'mouse su' muove verso Z negativa (inverte il verso).")]
    [SerializeField] private bool invertDirection = false;

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    // Stato
    private Vector3 basePoint; // posizione iniziale in mondo
    private float zCurrent;    // offset lungo Z rispetto a basePoint
    private float zTarget;

    private void Awake()
    {
        basePoint = transform.position;
        zCurrent = transform.position.z - basePoint.z;
        zTarget = Mathf.Clamp(zCurrent, zLimits.x, zLimits.y);

        if (Mathf.Approximately(zLimits.x, zLimits.y))
        {
            Debug.LogWarning("[PlankMover_Z_NoHold_Transform] zLimits min==max: nessuna corsa disponibile.");
        }
    }

    private void OnEnable()
    {
        if (mouseDeltaAction && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();
        else
            Debug.LogWarning("[PlankMover_Z_NoHold_Transform] mouseDeltaAction non assegnata o action null.");
    }

    private void OnDisable()
    {
        if (mouseDeltaAction && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Disable();
    }

    private void Update()
    {
        if (mouseDeltaAction == null || mouseDeltaAction.action == null) return;

        // 1) Delta del mouse (pixel/frame)
        Vector2 d = mouseDeltaAction.action.ReadValue<Vector2>();
        float dY = d.y;

        // 2) Normalizza per altezza schermo -> frazione schermo per frame
        float frac = dY / Mathf.Max(1f, (float)Screen.height);

        // 3) Converte in metri/secondo e integra con unscaledDeltaTime (indipendente da FPS/timeScale)
        float sign = invertDirection ? -1f : 1f;
        float unitsPerSecond = frac * rangePerScreen * sign;

        zTarget = Mathf.Clamp(zTarget + unitsPerSecond * Time.unscaledDeltaTime, zLimits.x, zLimits.y);

        // 4) Smoothing esponenziale
        float alpha = 1f - Mathf.Exp(-smoothing * Time.unscaledDeltaTime);
        zCurrent = Mathf.Lerp(zCurrent, zTarget, alpha);

        // 5) Applica posizione (solo lungo Z, X/Y restano alla base)
        transform.position = new Vector3(basePoint.x, basePoint.y, basePoint.z + zCurrent);

        if (logDebug && Time.frameCount % 20 == 0)
            Debug.Log($"[PlankMover_Z_NoHold_Transform] dY={dY:F1} z={zCurrent:F3}->{zTarget:F3}");
    }

    /// <summary>
    /// Se sposti il pezzo in runtime e vuoi che quella diventi la nuova "origine" della corsa, chiamala.
    /// </summary>
    public void RebaseHere()
    {
        basePoint = transform.position;
        zCurrent = 0f;
        zTarget = Mathf.Clamp(0f, zLimits.x, zLimits.y);
    }
#endif
}
