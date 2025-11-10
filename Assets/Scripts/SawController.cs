using UnityEngine;
using UnityEngine.InputSystem;

public class SawController : MonoBehaviour
{
    [Header("Riferimenti")]
    [Tooltip("Transform della sega; se vuoto usa questo transform")]
    [SerializeField] private Transform saw;

    [Header("Input")]
    [Tooltip("Azione di delta mouse (es. MinigameMouseDelta)")]
    [SerializeField] private InputActionReference mouseDeltaAction;

    [Header("Movimento lungo Z locale")]
    [Tooltip("Z min/max relativo alla posizione iniziale")]
    [SerializeField] private float minZ = -0.5f;
    [SerializeField] private float maxZ = 0.5f;

    [Header("Controllo")]
    [Tooltip("Sensibilità base del delta mouse")]
    [SerializeField] private float sensitivity = 0.08f;

    [Tooltip("Scala per framerate (moltiplica per deltaTime); metti 0 per disattivare")]
    [SerializeField] private float deltaTimeScale = 1f;

    [Tooltip("Deadzone per ignorare micro-jitter del mouse")]
    [SerializeField] private float deadzone = 0.01f;

    [Tooltip("Usa la Y del mouse invece di X")]
    [SerializeField] private bool useVerticalMouse = false;

    [Tooltip("Inverti direzione")]
    [SerializeField] private bool invert = false;

    [Header("Smoothing / Dinamica")]
    [Tooltip("0 = nessuno smoothing; valori alti = più morbido")]
    [Range(0f, 20f)]
    [SerializeField] private float smooth = 10f;

    [Tooltip("Accelera in base all'intensità del movimento (0 = off)")]
    [Range(0f, 2f)]
    [SerializeField] private float acceleration = 0.0f;

    // Stato interno
    private Vector3 _startLocalPos;
    private float _currentZ; // posizione "fisica" applicata (dopo smoothing)
    private float _targetZ;  // target calcolato dal delta mouse

    private void Awake()
    {
        if (saw == null) saw = transform;
    }

    private void OnEnable()
    {
        _startLocalPos = saw.localPosition;
        // Partenza centrata nel range
        _currentZ = _targetZ = Mathf.Clamp(0f, minZ, maxZ);

        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();

        // Posizione istantanea all'avvio
        ApplyLocalZInstant(_currentZ);
    }

    private void OnDisable()
    {
        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Disable();
    }

    private void Update()
    {
        if (mouseDeltaAction == null || mouseDeltaAction.action == null) return;

        // 1) Leggi delta e scegli asse
        Vector2 delta = mouseDeltaAction.action.ReadValue<Vector2>();
        float input = useVerticalMouse ? delta.y : delta.x;

        // 2) Deadzone per eliminare micro jitter
        if (Mathf.Abs(input) < deadzone) input = 0f;

        // 3) Inversione opzionale
        if (invert) input = -input;

        // 4) Acceleration opzionale: rende più "reattivo" per movimenti ampi
        if (acceleration > 0f)
        {
            // fattore cresce con l'intensità del delta
            float accFactor = 1f + acceleration * Mathf.Clamp01(Mathf.Abs(input));
            input *= accFactor;
        }

        // 5) Scala per sensibilità e, opzionalmente, per deltaTime
        float scale = sensitivity;
        if (deltaTimeScale > 0f)
            scale *= Time.deltaTime * Mathf.Max(deltaTimeScale, 0f);

        // 6) Aggiorna target e clamp
        _targetZ += input * scale;
        _targetZ = Mathf.Clamp(_targetZ, minZ, maxZ);

        // 7) Smoothing esponenziale
        if (smooth > 0f)
            _currentZ = Mathf.Lerp(_currentZ, _targetZ, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        else
            _currentZ = _targetZ;

        // 8) Applica posizione locale
        ApplyLocalZInstant(_currentZ);
    }

    private void ApplyLocalZInstant(float z)
    {
        Vector3 lp = _startLocalPos;
        lp.z += z;
        saw.localPosition = lp;
    }

    // --- Utility ---
    public void SetRange(float min, float max)
    {
        minZ = min; maxZ = max;
        _targetZ = Mathf.Clamp(_targetZ, minZ, maxZ);
        _currentZ = Mathf.Clamp(_currentZ, minZ, maxZ);
        ApplyLocalZInstant(_currentZ);
    }

    public void Recenter()
    {
        _targetZ = _currentZ = Mathf.Clamp(0f, minZ, maxZ);
        ApplyLocalZInstant(_currentZ);
    }
}