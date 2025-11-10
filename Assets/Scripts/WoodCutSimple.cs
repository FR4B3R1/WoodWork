using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class WoodCutSimple : MonoBehaviour
{
    [Header("Input (Mouse Delta)")]
    [SerializeField] private InputActionReference mouseDeltaAction; // <Mouse>/delta

    [Header("Taglio - Parametri")]
    [SerializeField] private float activationThreshold = 0.08f;  // soglia per contare attività (|delta.y|)
    [SerializeField] private float requiredActiveSeconds = 10.0f; // secondi richiesti di movimento attivo
    [SerializeField] private float deltaTimeScale = 1f;          // 1 = indipendente da FPS

    [Header("Eventi")]
    public UnityEvent OnCutComplete;

    [Header("Sicurezza (velocità)")]
    [SerializeField] private float safetySpeedThreshold = 1.5f;  // soglia warning
    [SerializeField] private float safetyMinExceedTime = 0.05f;  // tempo minimo sopra soglia
    [SerializeField] private SafetyWarningUI safetyUI;
    [SerializeField] private string safetyMessage = "Fai attenzione!";

    private float _activeSeconds;
    private bool _completed;

    private float _exceedTimer; // accumula tempo sopra soglia

    public float Progress01 => Mathf.Clamp01(_activeSeconds / Mathf.Max(0.001f, requiredActiveSeconds));

    private void OnEnable()
    {
        _activeSeconds = 0f;
        _completed = false;
        _exceedTimer = 0f;

        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();
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
        float absY = Mathf.Abs(delta.y);

        // --- Sicurezza ---
        if (absY >= safetySpeedThreshold)
        {
            _exceedTimer += Time.deltaTime;
            if (_exceedTimer >= safetyMinExceedTime)
            {
                safetyUI?.ShowWarning(safetyMessage);
                _exceedTimer = 0f; // evita spam frame-by-frame (UI ha già cooldown)
            }
        }
        else
        {
            _exceedTimer = 0f;
        }

        // --- Progresso taglio (senza decay) ---
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
    }

    // Utility
    public void ResetProgress()
    {
        _activeSeconds = 0f;
        _completed = false;
        _exceedTimer = 0f;
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