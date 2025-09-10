using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace JacatGames.Tracking
{
    public class DataBucketUploader : MonoBehaviour
    {
        [Serializable]
        public class Settings
        {
            [SerializeField] public bool LogSessionId = true;
            [SerializeField] public bool LogDeviceId = true;
            public bool LogPayloadToConsole = true;
        }

        [SerializeField] private Settings _logSettings;

        private string _url = "https://ingest.databuckets.com/push";

        private string _apiKey;

        private HttpClient _httpClient;
        private Dictionary<string, object> _eventProperties = new Dictionary<string, object>();

        private static DataBucketUploader _instance;

        private static DataBucketUploader Instance
        {
            get
            {
                if (_instance == null)
                {
                    var gO = new GameObject("DataBucketUploader_AutoCreated");
                    DontDestroyOnLoad(gO);
                    _instance = gO.AddComponent<DataBucketUploader>();
                }

                return _instance;
            }
        }

        public Settings LogSettings
        {
            get
            {
                if (_logSettings == null)
                {
                    _logSettings = new();
                }

                return _logSettings;
            }
        }

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
            }

            FirebaseManager.CheckWaitForReady(OnFirebaseInitialized);
        }

        private void OnFirebaseInitialized(object sender, bool success)
        {
            if (!success) { return; }

            Initialize();
        }

        void Initialize()
        {
            _httpClient = new HttpClient();
            _apiKey = GetSettingsApiKey();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
            // Initialize default properties
            _eventProperties["_ts"] = 0;
            _eventProperties["app_id"] = Application.identifier;
            #if UNITY_EDITOR
            _eventProperties["app_version"] = $"{Application.version}-editor";
            #else
            _eventProperties["app_version"] = Application.version;
            #endif
            _eventProperties["app_name"] = Application.productName;
            _eventProperties["country"] = "Vietnam";

            if (LogSettings.LogSessionId)
            {
                FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig((sender, success) =>
                {
                    string installationId = FirebaseRemoteConfigHelper.instance.GetFirebaseInstallationId();
                    if (!string.IsNullOrEmpty(installationId))
                    {
                        int firstDotIndex = installationId.IndexOf('.');
                        if (firstDotIndex >= 0) // Dot found
                        {
                            installationId = installationId.Substring(firstDotIndex + 1);
                        }
                    }

                    _eventProperties["user_pseudo_id"] = installationId;
                    _eventProperties["session_id"] = GenerateSessionId(installationId);
                });
            }

            if (LogSettings.LogDeviceId)
            {
                _eventProperties["device_id"] = SystemInfo.deviceUniqueIdentifier;
            }
        }
        
        private void OnDestroy()
        {
            _httpClient.Dispose();
        }

        private async void SendDataEvent(string eventName, params (string key, object value)[] parameters)
        {
            //check fix api key, weird bug in build
            if (string.IsNullOrEmpty(_apiKey))
            {
                if (!string.IsNullOrEmpty(GetSettingsApiKey()))
                {
                    if (_httpClient.DefaultRequestHeaders.Contains("X-API-KEY"))
                    {
                        _httpClient.DefaultRequestHeaders.Remove("X-API-KEY");
                    }

                    _apiKey = GetSettingsApiKey();
                    _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);
                }

                Debug.Log($"Api key is empty, try getting correct API key: {_apiKey}");
            }

            if (string.IsNullOrEmpty(_apiKey))
            {
                Debug.LogError($"DataBucket API key is empty. Please set API key in Firebase Manager's setting. Send event failed.");
            }

            long nowMillis = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // build base payload
            var payloadDict = new Dictionary<string, object>(_eventProperties)
            {
                ["_ts"] = nowMillis, // update timestamp per event
                ["event_name"] = eventName
            };

            // add custom parameters if any
            // edit: only log event name
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    payloadDict[kvp.key] = kvp.value;
                }
            }

            // serialize final payload
            string jsonPayload = JsonConvert.SerializeObject(payloadDict);
            if (LogSettings.LogPayloadToConsole)
            {
                Debug.Log($">event: {eventName} ({DateTime.Now:HH:mm:ss.f}): {jsonPayload}");
            }

            var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = await _httpClient.PostAsync(_url, content);
                string responseText = await response.Content.ReadAsStringAsync();

                // Debug.Log($"[HTTP] Status: {response.StatusCode}, Response: {responseText}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[HTTP] Exception: {ex}");
            }
        }

        static string GetSettingsApiKey()
        {
            return FirebaseManager.instance.DataBucketApiKey;
        }

        public static void LogEvent(string name, string paramName, int value)
        {
            LogEvent(name, new (string, object)[] { (paramName, value) });
        }

        public static void LogEvent(string name, string paramName, double value)
        {
            LogEvent(name, new (string, object)[] { (paramName, value) });
        }

        public static void LogEvent(string name, string paramName, string value)
        {
            LogEvent(name, new (string, object)[] { (paramName, value) });
        }

        public static void LogEvent(string name)
        {
            LogEvent(name, Array.Empty<(string, object)>());
        }

        public static void LogEvent(string name, params (string, object)[] parameters)
        {
            #if DEBUG_EVENT
        instance.SendDataEvent("debug_" + name, parameters);
            #else
            Instance.SendDataEvent(name, parameters);
            #endif
        }

        // The new LogEvent method with ParameterData[] as the signature
        public static void LogEvent(string name, params ParameterData[] parameters)
        {
            // Convert ParameterData[] to (string, object)[] for DataBucket
            var parameterList = new List<(string, object)>();

            foreach (var param in parameters)
            {
                parameterList.Add((param.Key, param.Value)); // Convert ParameterData to key-value pair
            }

            #if DEBUG_EVENT
        instance.SendDataEvent("debug_" + name, parameterList.ToArray());
            #else
            Instance.SendDataEvent(name, parameterList.ToArray());
            #endif
        }

        public static void SetUserProperties(string name, string value)
        {
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogError($"name is null");
            }

            Instance._eventProperties[name] = value;
        }

        public static string GenerateSessionId(string userId)
        {
            // string shortGuid = Guid.NewGuid().ToString("N").Substring(0, 8);
            long unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return $"p:{userId}:{unixTimestamp}";
        }
    }
}