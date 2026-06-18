using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class InfoManager : MonoBehaviour
{
    [SerializeField] private GameObject infoCanvas;

    [HideInInspector] public bool isActive = false;

    private void Update()
    {
        if (isActive && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            infoCanvas.SetActive(false);
            isActive = false;
        }
    }

    public void ToggleInfoCanvas()
    {
        if (infoCanvas != null)
        {
            infoCanvas.SetActive(!isActive);
            isActive = !isActive;
        }
        else
        {
            Debug.LogWarning("[InfoManager] Пожалуйста, назначьте Info Canvas Object в инспекторе скрипта.", this);
        }
    }
}
