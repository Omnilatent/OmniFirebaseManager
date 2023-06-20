using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading.Tasks;
using System;

#if !DISABLE_FIREBASE
using Firebase.RemoteConfig;
using Firebase.Extensions;
#endif

public class FirebaseRemoteConfigHelper : MonoBehaviour
{
#if !DISABLE_FIREBASE
    public static System.EventHandler<bool> onFetchComplete;
    string firebaseInstanceId;

    public static FirebaseRemoteConfigHelper instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        if (FirebaseManager.FirebaseReady) OnFirebaseReady(this, true);
        else FirebaseManager.handleOnReady += OnFirebaseReady;
    }

    [SerializeField] bool showFirebaseInstanceId = false;
    static bool? initSuccess;
    public static bool HasInitialized(bool logError = true)
    {
        bool returnVal = initSuccess.HasValue ? initSuccess.Value : false;
        if (!returnVal && logError)
        {
            Debug.Log("Firebase Remote has not initialized successfully.");
        }
        return returnVal;
    }

    void OnFirebaseReady(object sender, bool isReady)
    {
        // initSuccess = isReady; //change: assign initSuccess to after fetched remote config
        if (isReady) Init();
        else
        {
            initSuccess = false;
        }
    }

    async void Init()
    {
        string id = await GetFirebaseInstallationIdAsync();
        if (Debug.isDebugBuild && showFirebaseInstanceId)
        {
            TextEditor te = new TextEditor();
            te.text = id;
            te.SelectAll();
            te.Copy();
            //Manager.Add(PopupController.POPUP_SCENE_NAME, new PopupData(PopupType.OK, System.String.Format("Instance ID Token {0}", id)));
        }
        if (FirebaseManager.FirebaseReady)
        {
            Debug.Log($"Firebase ready: {FirebaseManager.FirebaseReady}");
            SetDefaultValues();
            // initSuccess = true;
        }

        if (Debug.isDebugBuild)
        {
            var setting = FirebaseRemoteConfig.DefaultInstance.ConfigSettings;
            //setting.IsDeveloperMode = true; //deprecated in Firebase 8.6.2
            setting.MinimumFetchInternalInMilliseconds = 2000;
        }

        FetchData();
    }

    public static int GetInt(string key, int defaultValue = 0)
    {
        //FetchData();
        //if (!HasInitialized()) Debug.Log("Firebase Remote: init not success");
        ConfigValue config = GetConfig(key);
        if (HasInitialized() && !string.IsNullOrEmpty(config.StringValue))
            return (int)config.DoubleValue;
        else return defaultValue;
    }

    public static float GetFloat(string key, float defaultValue = 0)
    {
        ConfigValue config = GetConfig(key);
        if (HasInitialized() && !string.IsNullOrEmpty(config.StringValue))
            return (float)config.DoubleValue;
        else return defaultValue;
    }

    public static bool GetBool(string key, bool defaultValue)
    {
        return HasInitialized() ? GetConfig(key).BooleanValue : defaultValue;
    }

    public static string GetString(string key, string defaultValue)
    {
        return HasInitialized() ? GetConfig(key).StringValue : defaultValue;
    }

    static ConfigValue GetConfig(string key)
    {
        return GetFirebaseInstance().GetValue(key);
    }

    //Since Firebase 7.2.0, get Firebase Installation Auth Token instead
    async Task<string> GetFirebaseInstallationIdAsync()
    {
        string result = null;
        if (FirebaseManager.FirebaseReady)
        {
            //Firebase.InstanceId.FirebaseInstanceId.GetInstanceId(FirebaseManager.app);
            /*await Firebase.Installations.FirebaseInstallations.DefaultInstance.GetIdAsync().ContinueWith(
                task =>
                {
                    if (!(task.IsCanceled || task.IsFaulted) && task.IsCompleted)
                    {
                        UnityEngine.Debug.Log(System.String.Format("Instance ID Token {0}", task.Result));
                        result = task.Result;
                    }
                });*/

            await Firebase.Installations.FirebaseInstallations.DefaultInstance.GetTokenAsync(false).ContinueWith(
                task =>
                {
                    if (!(task.IsCanceled || task.IsFaulted) && task.IsCompleted)
                    {
                        UnityEngine.Debug.Log(System.String.Format("Installations token {0}", task.Result));
                        firebaseInstanceId = result = task.Result;
                    }
                });

            /*await Firebase.InstanceId.FirebaseInstanceId.DefaultInstance.GetTokenAsync().ContinueWith(
            task =>
            {
                if (!(task.IsCanceled || task.IsFaulted) && task.IsCompleted)
                {
                    UnityEngine.Debug.Log(System.String.Format("Instance ID Token {0}", task.Result));
                    result = task.Result;
                }
            });*/
        }
        return result;
    }

    public string GetFirebaseInstallationId()
    {
        HasInitialized();
        return firebaseInstanceId;
    }

    static void FetchData()
    {
        if (FirebaseManager.FirebaseReady)
        {
            FetchDataAsync((task) =>
            {
                if (task != null && task.IsCompleted)
                {
                    Debug.Log("Fetch async done");
                    //FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
                    initSuccess = true;
                    onFetchComplete?.Invoke(null, true);
                }
                else
                {
                    Debug.Log("Fetch async failed. Either because Firebase fetch failed or an exception was thrown in callbacks");
                    initSuccess = false;
                    onFetchComplete?.Invoke(null, false);
                }
            });
        }
    }

    // Start a fetch request.
    // FetchAsync only fetches new data if the current data is older than the provided
    // timespan.  Otherwise it assumes the data is "recent enough", and does nothing.
    // By default the timespan is 12 hours, and for production apps, this is a good
    // number. For this example though, it's set to a timespan of zero, so that
    // changes in the console will always show up immediately.
    async static void FetchDataAsync(Action<Task> FetchComplete)
    {
        Debug.Log("Fetching data...");
        try
        {
            TimeSpan expireDuration = Debug.isDebugBuild ? TimeSpan.Zero : new TimeSpan(12, 0, 0);
            System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAsync(expireDuration);
            await fetchTask;
            System.Threading.Tasks.Task activateFetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.ActivateAsync();
            await activateFetchTask.ContinueWithOnMainThread(FetchComplete);
        }
        catch (System.Exception e)
        {
            Debug.LogWarning(e);
            Firebase.FirebaseException firebaseException = e as Firebase.FirebaseException;
            if (firebaseException != null)
            {
                string errorCodeMsg = $"Firebase Error: code {firebaseException.ErrorCode}, {firebaseException.Message}";
                Debug.LogWarning(errorCodeMsg);
                if (firebaseException.ErrorCode == 1)
                {
                    FirebaseManager.LogEvent("FirebaseRCH_Fetch_ErrorCode_1", "message", firebaseException.Message);
                }
                else
                {
                    FirebaseManager.LogCrashlytics(errorCodeMsg);
                    FirebaseManager.LogException(firebaseException);
                }
            }
            else
            {
                FirebaseManager.LogException(e);
            }
            FetchComplete(null);
        }
        /*System.Threading.Tasks.Task fetchTask = Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.FetchAndActivateAsync();
        return fetchTask.ContinueWithOnMainThread(FetchComplete);*/
    }

    void SetDefaultValues()
    {
        System.Collections.Generic.Dictionary<string, object> defaults = new System.Collections.Generic.Dictionary<string, object>();

        //defaults.Add(Const.RMCF_TIME_BETWEEN_ADS, AdsManager.TIME_BETWEEN_ADS);
        // defaults.Add(RemoteConfigAdsPlacement.instance.adsPlacementConfigKey, "");

        Firebase.RemoteConfig.FirebaseRemoteConfig.DefaultInstance.SetDefaultsAsync(defaults);
    }

    /// <summary>
    /// Check if Remote Config has fetched successfully. If yes, return callback immediately, else add in to delegate to wait for callback
    /// </summary>
    public static void CheckAndHandleFetchConfig(System.EventHandler<bool> callback)
    {
        if (initSuccess.HasValue) callback(null, HasInitialized());
        else onFetchComplete += callback;
    }

    static FirebaseRemoteConfig GetFirebaseInstance()
    {
        return FirebaseRemoteConfig.DefaultInstance;
    }
#endif
}
