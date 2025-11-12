using UnityEngine;
using UnityEngine.InputSystem;
using StarterAssets;
using System.Collections;

public class CameraSwitcherSecondMinigame : MonoBehaviour
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
    [SerializeField] private MonoBehaviour woodController; //script dell'asse di legno (es. WoodMover)

    [Header("UI Minigioco")]
    [SerializeField] private MinigameUIController minigameUI;

    [Header("Popup di avviso")]
    [SerializeField] private GameObject warningText; // Assegna il Panel di avviso
    [SerializeField] private float warningDuration = 3f; // Durata in secondi

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
            ExitToMainCamera();
        else
            EnterMinigameView();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minigame2") && inventoryManager.allEquipped == true)
            _nearMinigame = true;
        else if (other.CompareTag("Minigame2") && inventoryManager.allEquipped == false)
            ShowWarning();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Minigame2"))
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
        // NON toccare l'AudioListener, lascialo sulla mainCamera o su un oggetto dedicato
    }

    private void SetMinigameLogicEnabled(bool enabled)
    {
        if (woodController != null) woodController.enabled = enabled;
    }

    void ShowWarning()
    {
        warningText.SetActive(true);
        Debug.Log("Non puoi entrare: devi indossare tutti i DPI");
        StopAllCoroutines(); // Evita conflitti se viene chiamato più volte
        StartCoroutine(HideWarningAfterDelay(warningDuration));
    }

    IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningText.SetActive(false);
    }
}
