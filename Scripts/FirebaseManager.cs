﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

/* CHANGELOG:
 * v1.1.0: 5/5: change CheckDependenciesAsync function to async to work with remote config manager
 * 1.1.1: 24/6/2020: Add LogException & Crashlytic Log
 * */
public class FirebaseManager : MonoBehaviour
{
    [Tooltip("If true, log event will also log to console when play in Editor")]
    [SerializeField] private bool logConsoleEditor = true;
    
    [Tooltip("If true, log event will also log to console when play in Player (Debug build)")]
    [SerializeField] private bool logConsolePlayerDebug = false; 

    public static FirebaseManager instance
    {
        get
        {
            if (_instance == null)
            {
                var prefab = Resources.Load<FirebaseManager>("FirebaseManager");
                if (prefab != null) _instance = Instantiate(prefab);
                else { Debug.LogWarning("FirebaseManager not found in Resources. Please import Firebase Manager's extra files."); }
            }

            return _instance;
        }
    }

    private static FirebaseManager _instance;

    public static Firebase.FirebaseApp app;

    static bool? firebaseReady;
    public static bool hasReportedReadyError = false;
    public static bool FirebaseReady
    {
        get => firebaseReady.GetValueOrDefault(false);
    }

    public static System.EventHandler<bool> handleOnReady;
    const string DebugPrefix = "Debug_";
    private static bool isDebugBuild = false;

    private void Awake()
    {
        isDebugBuild = Debug.isDebugBuild;
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(this.gameObject);

            CheckGooglePlayService();
        }
    }

    static bool CheckInit() //check if firebase has been initiated correctly
    {
        if (!firebaseReady.HasValue)
        {
            if (!hasReportedReadyError)
            {
                Debug.LogError("Firebase ready hasn't been set. Check if a firebase instance existed.");
                hasReportedReadyError = true;
            }
        }
        return firebaseReady.GetValueOrDefault(false);
    }

    public static void SetCustomKey(string key, string value)
    {
        if (CheckInit())
        {
            Crashlytics.SetCustomKey(key, value);
        }
    }

    public static void LogCrashlytics(string message) { if (CheckInit()) Crashlytics.Log(message); }

    public static void LogException(System.Exception exception, bool logToConsole = true)
    {
        if (CheckInit()) { Crashlytics.LogException(exception); }
        if (logToConsole) Debug.LogException(exception);
    }

    public static void CheckGooglePlayService()
    {
        CheckDependenciesAsync();
    }

    static async void CheckDependenciesAsync()
    {
        await Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(taskCheck =>
        {
            var dependencyStatus = taskCheck.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                FirebaseManager.app = Firebase.FirebaseApp.DefaultInstance;
                firebaseReady = true;
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format("Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                firebaseReady = false;
            }
        });
        handleOnReady?.Invoke(null, FirebaseReady);
    }

    public static void LogGameEvent(string paramName, string value)
    {
#if DEBUG_EVENT
        LogEvent("game_event_debug", paramName, value);
#else
        LogEvent("game_event_release", paramName, value);
#endif
    }

    public static void LogScreenView(string screenName, string screenClass = "Main")
    {
        if (!FirebaseManager.CheckInit())
        {
            print("Firebase not ready");
            return;
        }
        FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventScreenView,
            new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterScreenName, screenName),
            new Firebase.Analytics.Parameter(FirebaseAnalytics.ParameterScreenClass, screenClass)
        );
        LogConsole(FirebaseAnalytics.EventScreenView, FirebaseAnalytics.ParameterScreenName, screenName);
    }

    public static void LogEvent(string name, string paramName, int value)
    {
        CheckEventNameValid(name, paramName);
        LogConsole(name, paramName, value);
        if (!FirebaseManager.CheckInit()) return;
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name, string paramName, double value)
    {
        CheckEventNameValid(name, paramName);
        LogConsole(name, paramName, value);
        if (!FirebaseManager.CheckInit()) return;
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name, string paramName, string value)
    {
        CheckEventNameValid(name, paramName, value);
        LogConsole(name, paramName, value);
        if (!FirebaseManager.CheckInit())
        {
            print("Firebase not ready");
            return;
        }
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name)
    {
        CheckEventNameValid(name);
        LogConsole(name);
        if (!FirebaseManager.CheckInit())
        {
            print("Firebase not ready");
            return;
        }
#if DEBUG_EVENT
        FirebaseAnalytics.LogEvent(DebugPrefix + name);
#else
        FirebaseAnalytics.LogEvent(name);
#endif
    }
    
    public static void LogEvent(string name, params Firebase.Analytics.Parameter[] array)
    {
        CheckEventNameValid(name);
        if (!FirebaseManager.CheckInit())
        {
            print("Firebase not ready");
            return;
        }
#if DEBUG_EVENT
            FirebaseAnalytics.LogEvent(DebugPrefix + name, array);
#else
        FirebaseAnalytics.LogEvent(name, array);
#endif
    }

    public static void SetUserProperties(string name, string property)
    {
        if (!FirebaseManager.CheckInit())
        {
            return;
        }
        Firebase.Analytics.FirebaseAnalytics.SetUserProperty(name, property);
    }

    /// <summary>
    /// Check if Firebase is ready. If yes, return callback immediately, else add in to delegate to wait for callback
    /// </summary>
    public static void CheckWaitForReady(System.EventHandler<bool> callback)
    {
        if (firebaseReady.HasValue) callback(null, FirebaseReady);
        else handleOnReady += callback;
    }

    static bool CheckEventNameValid(string eventName, string paramName = null, string value = null)
    {
        bool isDebugging = isDebugBuild;
        bool isValid = true;
#if UNITY_EDITOR
        isDebugging = true;
#endif
        if (isDebugging)
        {
            string regexPattern = @"^[a-zA-Z]\w+$";
            if (eventName.Length > 40 || (!string.IsNullOrEmpty(paramName) && paramName.Length > 40))
            {
                var e = new System.ArgumentException($"Event '{eventName}', '{paramName}' name exceeds 40 characters");
                FirebaseManager.LogException(e, true);
                isValid = false;
            }

            if (!string.IsNullOrEmpty(value) && value.Length > 100)
            {
                var e = new System.ArgumentException($"Event '{eventName}', '{paramName}', param value '{value}' exceeds 100 characters");
                FirebaseManager.LogException(e, true);
                isValid = false;
            }
            
            if (!Regex.Match(eventName, regexPattern).Success || (!string.IsNullOrEmpty(paramName) && !Regex.Match(paramName, regexPattern).Success))
            {
                var e = new System.ArgumentException($"Event '{eventName}' with param '{paramName}' contains invalid characters");
                FirebaseManager.LogException(e, true);
                isValid = false;
            }
        }
        return isValid;
    }

    static void LogConsole(string name, string paramName = "", object value = null)
    {
        if (!isDebugBuild)
        {
            return;
        }
        
        bool doLog = false;
        #if UNITY_EDITOR
        doLog = instance.logConsoleEditor;
        #else
        doLog = instance.logConsolePlayerDebug;
        #endif
        if(doLog)
        {
            Debug.Log($"<color=yellow>firebase log:</color> {name}, {paramName}, {value}");
        }
    }
}
