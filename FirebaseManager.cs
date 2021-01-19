using SS.View;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Analytics;
using Firebase.Crashlytics;
using System.Threading.Tasks;

/* CHANGELOG:
 * v1.1.0: 5/5: change CheckDependenciesAsync function to async to work with remote config manager
 * 1.1.1: 24/6/2020: Add LogException & Crashlytic Log
 * */
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager instance { get; protected set; }
    public static Firebase.FirebaseApp app;

    static bool? firebaseReady;
    public static bool hasReportedReadyError = false;
    public static bool FirebaseReady
    {
        get => firebaseReady.GetValueOrDefault(false);
    }

    public static System.EventHandler<bool> handleOnReady;
    const string DebugPrefix = "Debug_";

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
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

    public static void LogException(System.Exception exception)
    {
        if (CheckInit()) { Crashlytics.LogException(exception); }
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
#if DEBUG_EVENT
        FirebaseAnalytics.SetCurrentScreen(DebugPrefix + screenName, screenClass);
#else
        FirebaseAnalytics.SetCurrentScreen(screenName, screenClass);
#endif
    }

    public static void LogEvent(string name, string paramName, int value)
    {
        if (!FirebaseManager.CheckInit()) return;
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name, string paramName, double value)
    {
        if (!FirebaseManager.CheckInit()) return;
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name, string paramName, string value)
    {
        if (!FirebaseManager.CheckInit())
        {
            print("Firebase not ready");
            return;
        }
        FirebaseAnalytics.LogEvent(name, paramName, value);
    }

    public static void LogEvent(string name)
    {
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
#if UNITY_EDITOR
        Debug.Log("<color=yellow>firebase log:</color> " + name);
#endif
    }
    public static void LogEvent(string name, Firebase.Analytics.Parameter[] array)
    {
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
}
