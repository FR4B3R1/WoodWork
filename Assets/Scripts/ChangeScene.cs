using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    [Header("Nome della scena di destinazione")]
    [SerializeField] private string targetScene = "SampleScene";

    [Header("Panel Menu")]
    [SerializeField] private GameObject panelMenu;

    [Header("Panel Task 1")]
    [SerializeField] private GameObject panelTask1;

    [Header("Panel Task 2")]
    [SerializeField] private GameObject panelTask2;

    [Header("Panel Descrizione Task")]
    [SerializeField] private GameObject panelDescrizione;

    private void Start()
    {
        panelMenu.SetActive(true);
        panelDescrizione.SetActive(false);
        panelTask1.SetActive(false);
        panelTask2.SetActive(false);

        Time.timeScale = 1f;
    }
    // Questo metodo sarà collegato al Button
    public void SwitchScene()
    {
        SceneManager.LoadScene(targetScene);
    }

    // metodo collegato al button per cambiare pannello (panel che mostra la task 1)
    public void ChangeToTask1()
    {

        panelMenu.SetActive(false);
        panelDescrizione.SetActive(false);
        panelTask1.SetActive(true);
        panelTask2.SetActive(false);

    }

    public void ChangeToTask2()
    {
        panelMenu.SetActive(false);
        panelDescrizione.SetActive(false);
        panelTask1.SetActive(false);
        panelTask2.SetActive(true);

    }

    public void BackToMenu()
    {
        panelMenu.SetActive(false);
        panelDescrizione.SetActive(true);
        panelTask1.SetActive(false);
        panelTask2.SetActive(false);

    }

    public void ChangeToDescriptionPanel()
    {
        panelMenu.SetActive(false);
        panelDescrizione.SetActive(true);
        panelTask1.SetActive(false);
        panelTask2.SetActive(false);
    }

    public void ApplicationQuit()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif


    }
}