using UnityEngine;
using UnityEngine.InputSystem;

public class MiniGameActivator : MonoBehaviour
{
    public Camera mainCamera;
    public Camera miniGameCamera;
    public MonoBehaviour playerControllerScript; // script di movimento (non il GameObject)
    [Header("Panel UI del minigioco")]
    [SerializeField] private GameObject panel; // Il root del pannello (il GameObject del Panel)

    private PlayerInput playerInput;
    private bool isNearMiniGameObject = false;
    private bool isInMiniGame = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Interaction"].performed += OnInteraction;
    }

    void OnDestroy()
    {
        playerInput.actions["Interaction"].performed -= OnInteraction;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Minigame"))
        {
            isNearMiniGameObject = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Minigame"))
        {
            isNearMiniGameObject = false;
        }
    }

    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (isNearMiniGameObject)
        {
            if (!isInMiniGame)
                EnterMiniGame();
            else
                ExitMiniGame();
        }
    }

    void EnterMiniGame()
    {
        mainCamera.enabled = false;
        miniGameCamera.enabled = true;

        playerControllerScript.enabled = false; // blocca movimento
        panel.SetActive(true);

        isInMiniGame = true;
    }

    void ExitMiniGame()
    {
        mainCamera.enabled = true;
        miniGameCamera.enabled = false;

        playerControllerScript.enabled = true; // riattiva movimento
        panel.SetActive(false);

        isInMiniGame = false;
    }
}