using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PopupManager : MonoBehaviour
{
    public GameObject popupPanel;
    public Image popupImage;

    public void ShowPopup(Sprite image, float duration)
    {
        popupImage.sprite = image;
        popupPanel.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(HidePopupAfterDelay(duration));
    }

    private IEnumerator HidePopupAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        popupPanel.SetActive(false);
    }
}
