using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WoodCutSimple : MonoBehaviour
{
    public enum SpeedAxis { X, Y, Magnitude }

    [Header("Input (Mouse Delta)")]
    [SerializeField] private InputActionReference mouseDeltaAction; // <Mouse>/delta

    [Header("Taglio - Parametri")]
    [Tooltip("Soglia minima dell'attività (su/giù) per accumulare progresso (per-frame, ma filtrata dal smoothing)")]
    [SerializeField] private float activationThreshold = 0.08f;
    [Tooltip("Secondi di movimento attivo necessari per completare il taglio")]
    [SerializeField] private float requiredActiveSeconds = 10.0f;
    [Tooltip("Scala opzionale per rendere l'accumulo indipendente dal frame rate (0=off, 1=usa deltaTime)")]
    [SerializeField] private float deltaTimeScale = 1f;

    [Header("Eventi")]
    public UnityEvent OnCutComplete;

    [Header("Sicurezza (velocità)")]
    [Tooltip("Asse da monitorare per la sicurezza (X, Y o Magnitude)")]
    [SerializeField] private SpeedAxis safetyAxis = SpeedAxis.Y;

    [Tooltip("Soglia di velocità in pixel/secondo oltre la quale mostrare il warning")]
    [SerializeField] private float safetySpeedThresholdPps = 900f;

    [Tooltip("Deadzone in pixel/secondo: sotto questo valore si considera 0 (evita jitter)")]
    [SerializeField] private float safetyDeadzonePps = 60f;

    [Tooltip("Durata minima (secondi, unscaled) sopra soglia prima di mostrare l’avviso")]
    [SerializeField] private float safetyMinExceedTime = 0.05f;

    [Tooltip("Fattore di smoothing (EMA) per stabilizzare la velocità [0..1] (0 = nessun filtro, 1 = molto lento)")]
    [Range(0f, 1f)]
    [SerializeField] private float safetySmoothing = 0.2f;

    [SerializeField] private SafetyWarningUI safetyUI;
    [SerializeField] private string safetyMessage = "Fai attenzione!";

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    // Stato taglio
    private float _activeSeconds;
    private bool _completed;

    // Stato sicurezza
    private float _exceedTimerUnscaled; // tempo sopra soglia, in unscaled seconds
    private float _smoothedPps;         // velocità smussata (px/s) sull’asse scelto

    public float Progress01 => Mathf.Clamp01(_activeSeconds / Mathf.Max(0.001f, requiredActiveSeconds));

    private void OnEnable()
    {
        _activeSeconds = 0f;
        _completed = false;
        _exceedTimerUnscaled = 0f;
        _smoothedPps = 0f;

        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();
        else if (logDebug)
            Debug.LogWarning("[WoodCutSimple] mouseDeltaAction non assegnata o action null.");
    }

    private void OnDisable()
    {
        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Disable();
    }

    private void Update()
    {
        if (_completed || mouseDeltaAction == null || mouseDeltaAction.action == null)
            return;

        // Leggi delta per frame
        Vector2 delta = mouseDeltaAction.action.ReadValue<Vector2>();

        // === 1) Calcolo velocità in px/s (indipendente da FPS) ===
        float dt = Mathf.Max(Time.unscaledDeltaTime, 1e-5f);
        float vX = Mathf.Abs(delta.x) / dt;
        float vY = Mathf.Abs(delta.y) / dt;
        float vMag = new Vector2(vX, vY).magnitude; // già in px/s

        // Scegli l’asse da usare per sicurezza
        float pps = safetyAxis switch
        {
            SpeedAxis.X => vX,
            SpeedAxis.Y => vY,
            _ => vMag
        };

        // Deadzone in px/s
        if (pps < safetyDeadzonePps) pps = 0f;

        // EMA smoothing
        if (safetySmoothing > 0f)
            _smoothedPps = Mathf.Lerp(pps, _smoothedPps, safetySmoothing);
        else
            _smoothedPps = pps;

        // === 2) Sicurezza: test soglia con tempo minimo (unscaled) ===
        if (_smoothedPps >= safetySpeedThresholdPps)
        {
            _exceedTimerUnscaled += Time.unscaledDeltaTime;
            if (_exceedTimerUnscaled >= safetyMinExceedTime)
            {
                if (safetyUI == null && logDebug)
                    Debug.LogWarning("[WoodCutSimple] safetyUI non assegnato: impossibile mostrare il warning.");
                safetyUI?.ShowWarning(safetyMessage);
                _exceedTimerUnscaled = 0f; // evita spam; il cooldown dello UI gestisce ulteriori ripetizioni
            }
        }
        else
        {
            _exceedTimerUnscaled = 0f;
        }

        // === 3) Progresso taglio (resta su asse Y come da tua logica) ===
        float absY = Mathf.Abs(delta.y); // per la “fatica” conti su/giù
        float timeScale = (deltaTimeScale > 0f) ? Time.deltaTime * deltaTimeScale : 1f;

        if (absY >= activationThreshold)
        {
            _activeSeconds += 1f * timeScale;
            if (_activeSeconds >= requiredActiveSeconds)
            {
                _activeSeconds = requiredActiveSeconds;
                _completed = true;
                OnCutComplete?.Invoke();
            }
        }

        if (logDebug && Time.frameCount % 15 == 0)
        {
            Debug.Log($"[WoodCutSimple] vX={vX:F0} pps, vY={vY:F0} pps, vMag={vMag:F0} pps, smoothed={_smoothedPps:F0} pps");
        }
    }

    // Utility
    public void ResetProgress()
    {
        _activeSeconds = 0f;
        _completed = false;
        _exceedTimerUnscaled = 0f;
        _smoothedPps = 0f;
    }

    public void ForceComplete()
    {
        _activeSeconds = requiredActiveSeconds;
        if (!_completed)
        {
            _completed = true;
            OnCutComplete?.Invoke();
        }
    }
}