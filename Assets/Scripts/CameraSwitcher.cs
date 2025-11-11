using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Camere")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Camera miniGameCamera;

    [Header("Input (Interact)")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Player (StarterAssets FPC)")]
    [SerializeField] private FirstPersonController firstPersonController;
    [SerializeField] private InventoryManager inventoryManager;

    [Header("Minigioco (logica)")]
    [SerializeField] private MonoBehaviour sawController;   // script della sega (es. SawController)
    [SerializeField] private MonoBehaviour woodCutSimple;   // script del progresso (es. WoodCutSimple)

    [Header("UI Minigioco")]
    [SerializeField] private MinigameUIController minigameUI;
    [SerializeField] private GameObject warningText;

    [Header("Cursor")]
    [SerializeField] private bool lockCursorDuringMinigame = true;

    private bool _nearMinigame;
    private bool _inMinigameView;
    private bool _minigameRunning;

    private float _originalMoveSpeed;
    private float _originalSprintSpeed;

    private void Awake()
    {
        if (firstPersonController != null)
        {
            _originalMoveSpeed = firstPersonController.MoveSpeed;
            _originalSprintSpeed = firstPersonController.SprintSpeed;
        }
    }

    private void OnEnable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed += OnInteract;
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null && interactAction.action != null)
        {
            interactAction.action.performed -= OnInteract;
            interactAction.action.Disable();
        }
        ExitToMainCamera(); // ripristino sicurezza
    }

    private void Start()
    {
        ExitToMainCamera(); // assicura main camera, panel nascosto, logica off
    }

    private void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!_nearMinigame) return;

        if (_inMinigameView)
        {
            ExitToMainCamera();
            return;
        }

        EnterMinigameView();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minigame"))
            _nearMinigame = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Minigame"))
        {
            _nearMinigame = false;
            if (_inMinigameView) ExitToMainCamera();
        }
    }

    // ===== Stati =====

    private void EnterMinigameView()
    {

        SetCameras(mainOn: false);
        _inMinigameView = true;

        // Blocca solo movimento player
        if (firstPersonController != null)
        {
            firstPersonController.MoveSpeed = 0f;
            firstPersonController.SprintSpeed = 0f;
        }

        // Disabilita logica finché non premi Start
        SetMinigameLogicEnabled(false);
        _minigameRunning = false;

        // Mostra pannello (parte nascosto)
        if (minigameUI != null)
            minigameUI.ShowPanel();

        // Cursor visibile per cliccare il bottone
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartMinigame()
    {


        // Questo metodo viene chiamato da MinigameUIController.OnStartPressed
        _minigameRunning = true;
        SetMinigameLogicEnabled(true);

        if (lockCursorDuringMinigame)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    private void ExitToMainCamera()
    {
        SetCameras(mainOn: true);
        _inMinigameView = false;
        _minigameRunning = false;

        // Logica minigioco off
        SetMinigameLogicEnabled(false);

        // Ripristina movimento
        if (firstPersonController != null)
        {
            firstPersonController.MoveSpeed = _originalMoveSpeed;
            firstPersonController.SprintSpeed = _originalSprintSpeed;
        }

        // Nascondi sempre il pannello quando esci
        if (minigameUI != null)
            minigameUI.HidePanel();

        // Stato cursor FPS
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void SetCameras(bool mainOn)
    {
        if (mainCamera) mainCamera.enabled = mainOn;
        if (miniGameCamera) miniGameCamera.enabled = !mainOn;
        ToggleAudioListener(mainCamera, mainOn);
        ToggleAudioListener(miniGameCamera, !mainOn);
    }

    private void SetMinigameLogicEnabled(bool enabled)
    {
        if (sawController != null) sawController.enabled = enabled;
        if (woodCutSimple != null) woodCutSimple.enabled = enabled;
    }

    private void ToggleAudioListener(Camera cam, bool enable)
    {
        if (!cam) return;
        var listener = cam.GetComponent<AudioListener>();
        if (listener) listener.enabled = enable;
    }
}
