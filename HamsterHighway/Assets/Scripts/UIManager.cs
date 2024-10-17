using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject invertGravityButton;

    public void SetInvertGravityButtonActivity(bool isActive)
    {
        invertGravityButton.SetActive(isActive);
    }
}
