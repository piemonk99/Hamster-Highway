using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField] private GameObject gravityInversionWidget;

    public void SetInvertGravityButtonActivity(bool isActive)
    {
        GameObject invertGravityButton = gravityInversionWidget.transform.Find("Gravity Button").gameObject;
        RectTransform inversionsRemainingText = gravityInversionWidget.transform.Find("Remaining Inversions").GetComponent<RectTransform>();

        if (isActive)
        {
            invertGravityButton.SetActive(true);
            inversionsRemainingText.anchoredPosition = new Vector2(0, inversionsRemainingText.anchoredPosition.y); // Set x position to -100
        }
        if (!isActive)
        {
            invertGravityButton.SetActive(false);
            inversionsRemainingText.anchoredPosition = new Vector2(-100, inversionsRemainingText.anchoredPosition.y); // Set x position to -100
        }
        
    }
}
