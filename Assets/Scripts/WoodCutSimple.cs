using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WoodCutSimple : MonoBehaviour
{
    public enum SpeedAxis { X, Y, Magnitude }

    [Header("Input (Mouse Delta)")]
    [SerializeField] private InputActionReference mouseDeltaAction; // <Mouse>/delta

    [Header("Taglio - Parametri")]
    [Tooltip("Soglia minima dell'attività (su/giù) per accumulare progresso")]
    [SerializeField] private float activationThreshold = 0.08f;
    [Tooltip("Secondi di movimento attivo necessari per completare il taglio")]
    [SerializeField] private float requiredActiveSeconds = 10.0f;
    [Tooltip("Scala opzionale per rendere l'accumulo indipendente dal frame rate")]
    [SerializeField] private float deltaTimeScale = 1f;

    [Header("Eventi")]
    public UnityEvent OnCutComplete;

    [Header("Sicurezza (velocità)")]
    [SerializeField] private SpeedAxis safetyAxis = SpeedAxis.Y;
    [SerializeField] private float safetySpeedThresholdPps = 900f;
    [SerializeField] private float safetyDeadzonePps = 60f;
    [SerializeField] private float safetyMinExceedTime = 0.05f;
    [Range(0f, 1f)]
    [SerializeField] private float safetySmoothing = 0.2f;
    [SerializeField] private SafetyWarningUI safetyUI;
    [SerializeField] private string safetyMessage = "Fai attenzione!";

    [Header("Effetto Particellare Continuo")]
    [SerializeField] private ParticleSystem cuttingParticles; // Particelle già presenti nella scena

    [Header("Debug")]
    [SerializeField] private bool logDebug = false;

    // Stato taglio
    private float _activeSeconds;
    private bool _completed;

    // Stato sicurezza
    private float _exceedTimerUnscaled;
    private float _smoothedPps;

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

        Vector2 delta = mouseDeltaAction.action.ReadValue<Vector2>();

        // === 1) Calcolo velocità in px/s ===
        float dt = Mathf.Max(Time.unscaledDeltaTime, 1e-5f);
        float vX = Mathf.Abs(delta.x) / dt;
        float vY = Mathf.Abs(delta.y) / dt;
        float vMag = new Vector2(vX, vY).magnitude;

        float pps = safetyAxis switch
        {
            SpeedAxis.X => vX,
            SpeedAxis.Y => vY,
            _ => vMag
        };

        if (pps < safetyDeadzonePps) pps = 0f;

        _smoothedPps = (safetySmoothing > 0f) ? Mathf.Lerp(pps, _smoothedPps, safetySmoothing) : pps;

        // === 2) Sicurezza ===
        if (_smoothedPps >= safetySpeedThresholdPps)
        {
            _exceedTimerUnscaled += Time.unscaledDeltaTime;
            if (_exceedTimerUnscaled >= safetyMinExceedTime)
            {
                safetyUI?.ShowWarning(safetyMessage);
                _exceedTimerUnscaled = 0f;
            }
        }
        else
        {
            _exceedTimerUnscaled = 0f;
        }

        // === 3) Progresso taglio ===
        float absY = Mathf.Abs(delta.y);
        float timeScale = (deltaTimeScale > 0f) ? Time.deltaTime * deltaTimeScale : 1f;

        bool isCutting = absY >= activationThreshold;

        if (isCutting)
        {
            _activeSeconds += 1f * timeScale;
            if (_activeSeconds >= requiredActiveSeconds)
            {
                _activeSeconds = requiredActiveSeconds;
                _completed = true;
                OnCutComplete?.Invoke();
            }
        }

        // === 4) Gestione particelle ===
        if (cuttingParticles != null)
        {
            var emission = cuttingParticles.emission;
            emission.enabled = isCutting && !_completed;
            emission.rateOverTime = absY * 100f; // più movimento = più particelle
        }

        if (logDebug && Time.frameCount % 15 == 0)
        {
            Debug.Log($"[WoodCutSimple] vX={vX:F0} pps, vY={vY:F0} pps, vMag={vMag:F0} pps, smoothed={_smoothedPps:F0} pps");
        }
    }

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