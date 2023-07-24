using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Firebase.RemoteConfig;
using LitJson;
using UnityEngine;

namespace Omnilatent.FirebaseManagerNS
{
    public class CacheRemoteConfigData
    {
        public Dictionary<string, CacheConfigValue> configData = new Dictionary<string, CacheConfigValue>();

        public void Init() { }
    }

    public class CacheConfigValue
    {
        public string Data;

        public CacheConfigValue(string data) { Data = data; }

        public string StringValue
        {
            get { return Data; }
        }

        public double DoubleValue => Convert.ToDouble(this.StringValue, (IFormatProvider)CultureInfo.InvariantCulture);

        public long LongValue => Convert.ToInt64(this.StringValue, (IFormatProvider)CultureInfo.InvariantCulture);

        public bool BooleanValue
        {
            get
            {
                string stringValue = this.StringValue;
                if (string.Equals(stringValue, "true"))
                    return true;
                if (string.Equals(stringValue, "false"))
                    return false;
                throw new FormatException(string.Format("ConfigValue '{0}' is not a boolean value", (object)stringValue));
            }
        }
    }

    public static class CacheRemoteConfig
    {
        public static CacheRemoteConfigData data;
        public const string PREF_KEY = "REMOTE_CONFIG_CACHE";

        static CacheRemoteConfig() { Load(); }

        public static void Load()
        {
            if (PlayerPrefs.HasKey(PREF_KEY))
            {
                string raw = PlayerPrefs.GetString(PREF_KEY);
                CacheRemoteConfigData value = JsonMapper.ToObject<CacheRemoteConfigData>(raw);
                data = value;
            }
            else
            {
                data = new CacheRemoteConfigData();
                data.Init();
            }

            FirebaseRemoteConfigHelper.CheckAndHandleFetchConfig((sender, success) =>
            {
                if (success)
                {
                    data.configData.Clear();
                    string log = "Config from firebase :\n";

                    foreach(var config in FirebaseRemoteConfigHelper.GetFirebaseInstance().AllValues)
                    {
                        string value = config.Value.StringValue;
                        data.configData.Add(config.Key, new CacheConfigValue(value));
                        log += $"{config.Key} : {config.Value}\n";
                    }

                    Debug.Log(log);
                    Save();
                }
            });
        }

        public static void Save()
        {
            var raw = JsonMapper.ToJson(data);
            PlayerPrefs.SetString(PREF_KEY, raw);
        }

        public static CacheConfigValue GetConfig(string key)
        {
            if (data.configData.TryGetValue(key, out var result)) { return result; }

            return null;
        }

        public static string GetConfig(string key, string defaultValue)
        {
            if (data.configData.TryGetValue(key, out var result)) { return result.StringValue; }
            else { return defaultValue; }
        }

        public static bool GetConfig(string key, bool defaultValue)
        {
            if (data.configData.TryGetValue(key, out var result)) { return result.BooleanValue; }
            else { return defaultValue; }
        }

        public static long GetConfig(string key, long defaultValue)
        {
            if (data.configData.TryGetValue(key, out var result)) { return result.LongValue; }
            else { return defaultValue; }
        }

        public static double GetConfig(string key, double defaultValue)
        {
            if (data.configData.TryGetValue(key, out var result)) { return result.DoubleValue; }
            else { return defaultValue; }
        }
    }
}