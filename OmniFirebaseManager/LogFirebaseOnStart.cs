using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LogFirebaseOnStart : MonoBehaviour
{
    [SerializeField] string eventName = "Unset";
    public enum Type { LogEvent = 0, ScreenName = 1}
    [SerializeField] Type m_Type;
    string screenClass = "Main";

    private void Start()
    {
        switch (m_Type)
        {
            case Type.LogEvent:
                FirebaseManager.LogEvent(eventName);
                break;
            case Type.ScreenName:
                FirebaseManager.LogScreenView(eventName, screenClass);
                break;
        }
        
    }
}
