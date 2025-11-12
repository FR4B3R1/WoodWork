using UnityEngine;
using UnityEngine.InputSystem;

public class SawController : MonoBehaviour
{
    [Header("Riferimenti")]
    [SerializeField] private Transform saw;

    [Header("Input")]
    [SerializeField] private InputActionReference mouseDeltaAction;

    [Header("Movimento lungo Z locale")]
    [SerializeField] private float minZ = -0.5f;
    [SerializeField] private float maxZ = 0.5f;

    [Header("Controllo")]
    [SerializeField] private float sensitivity = 0.08f;
    [SerializeField] private float deltaTimeScale = 1f;
    [SerializeField] private float deadzone = 0.01f;
    [SerializeField] private bool useVerticalMouse = false;
    [SerializeField] private bool invert = false;

    [Header("Smoothing / Dinamica")]
    [Range(0f, 20f)][SerializeField] private float smooth = 10f;
    [Range(0f, 2f)][SerializeField] private float acceleration = 0.0f;

    [Header("Audio Sega")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sawSound;

    private Vector3 _startLocalPos;
    private float _currentZ;
    private float _targetZ;

    private bool isSawMoving = false;

    private void Awake()
    {
        if (saw == null) saw = transform;
    }

    private void OnEnable()
    {
        _startLocalPos = saw.localPosition;
        _currentZ = _targetZ = Mathf.Clamp(0f, minZ, maxZ);

        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Enable();

        ApplyLocalZInstant(_currentZ);

        if (audioSource != null)
        {
            audioSource.clip = sawSound;
            audioSource.loop = true; // Suono continuo mentre si muove
        }
    }

    private void OnDisable()
    {
        if (mouseDeltaAction != null && mouseDeltaAction.action != null)
            mouseDeltaAction.action.Disable();

        if (audioSource != null) audioSource.Stop();
    }

    private void Update()
    {
        if (mouseDeltaAction == null || mouseDeltaAction.action == null) return;

        Vector2 delta = mouseDeltaAction.action.ReadValue<Vector2>();
        float input = useVerticalMouse ? delta.y : delta.x;

        if (Mathf.Abs(input) < deadzone) input = 0f;
        if (invert) input = -input;

        if (acceleration > 0f)
        {
            float accFactor = 1f + acceleration * Mathf.Clamp01(Mathf.Abs(input));
            input *= accFactor;
        }

        float scale = sensitivity;
        if (deltaTimeScale > 0f)
            scale *= Time.deltaTime * Mathf.Max(deltaTimeScale, 0f);

        _targetZ += input * scale;
        _targetZ = Mathf.Clamp(_targetZ, minZ, maxZ);

        if (smooth > 0f)
            _currentZ = Mathf.Lerp(_currentZ, _targetZ, 1f - Mathf.Exp(-smooth * Time.deltaTime));
        else
            _currentZ = _targetZ;

        ApplyLocalZInstant(_currentZ);

        // --- LOGICA MOVIMENTO ---
        isSawMoving = Mathf.Abs(input) > 0f;

        // --- AUDIO ---
        if (audioSource != null)
        {
            if (isSawMoving)
            {
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }
    }

    private void ApplyLocalZInstant(float z)
    {
        Vector3 lp = _startLocalPos;
        lp.z += z;
        saw.localPosition = lp;
    }

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