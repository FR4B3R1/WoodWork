using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class MiniGameActivator : MonoBehaviour
{
    public Camera mainCamera;
    public Camera miniGameCamera;
    public MonoBehaviour playerControllerScript;
    [Header("Panel UI del minigioco")]
    [SerializeField] private GameObject panel;

    [Header("Popup di avviso")]
    [SerializeField] private GameObject warningPopup; // Assegna il Panel di avviso
    [SerializeField] private float warningDuration = 3f; // Durata in secondi

    private PlayerInput playerInput;
    private bool isNearMiniGameObject = false;
    private bool isInMiniGame = false;

    private InventoryManager inventory;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.actions["Interaction"].performed += OnInteraction;

        inventory = GetComponent<InventoryManager>();
        warningPopup.SetActive(false); // Nascondi all'inizio
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
            warningPopup.SetActive(false);
        }
    }

    private void OnInteraction(InputAction.CallbackContext context)
    {
        if (isNearMiniGameObject)
        {
            if (!isInMiniGame)
            {
                if (inventory != null && inventory.allEquipped)
                {
                    EnterMiniGame();
                }
                else
                {
                    ShowWarning();
                }
            }
            else
            {
                ExitMiniGame();
            }
        }
    }

    void EnterMiniGame()
    {
        mainCamera.enabled = false;
        miniGameCamera.enabled = true;

        playerControllerScript.enabled = false;
        panel.SetActive(true);

        isInMiniGame = true;
        warningPopup.SetActive(false);
    }

    void ExitMiniGame()
    {
        mainCamera.enabled = true;
        miniGameCamera.enabled = false;

        playerControllerScript.enabled = true;
        panel.SetActive(false);

        isInMiniGame = false;
    }

    void ShowWarning()
    {
        warningPopup.SetActive(true);
        Debug.Log("Non puoi entrare: devi indossare tutti i DPI");
        StopAllCoroutines(); // Evita conflitti se viene chiamato più volte
        StartCoroutine(HideWarningAfterDelay(warningDuration));
    }

    IEnumerator HideWarningAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        warningPopup.SetActive(false);
    }
}
