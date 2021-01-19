using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogFirebaseEventOnClick : MonoBehaviour
{
    [SerializeField] string eventName = "Unset";
    [SerializeField] bool autoAddListener = true;
    [SerializeField] string eventName_ToggleOff;
    public enum UIType { Button = 0, Toggle = 1 }
    public UIType uiType;

    private void Start()
    {
        if (autoAddListener)
        {
            switch (uiType)
            {
                case UIType.Button:
                    Button button = GetComponent<Button>();
                    button.onClick.AddListener(LogClick);
                    break;
                case UIType.Toggle:
                    Toggle toggle = GetComponent<Toggle>();
                    toggle.onValueChanged.AddListener(LogClick);
                    break;
            }
        }
    }

    public void LogClick()
    {
        FirebaseManager.LogEvent(eventName);
    }

    public void LogClick(bool isOn)
    {
        if (isOn)
            FirebaseManager.LogEvent(eventName);
        else FirebaseManager.LogEvent(eventName_ToggleOff);
    }

    private void OnValidate()
    {
        if (autoAddListener)
        {
            switch (uiType)
            {
                case UIType.Button:
                    Button button = GetComponent<Button>();
                    if (button == null)
                    {
                        print($"{gameObject} LogFirebaseEventOnClick No button component");
                    }
                    break;
                case UIType.Toggle:
                    Toggle toggle = GetComponent<Toggle>();
                    if (toggle == null)
                    {
                        print($"{gameObject} LogFirebaseEventOnClick No toggle component");
                    }
                    break;
            }
        }
    }
}
