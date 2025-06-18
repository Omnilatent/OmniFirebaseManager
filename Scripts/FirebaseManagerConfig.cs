using System.Collections.Generic;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Omnilatent.FirebaseManagerNS
{
    [CreateAssetMenu(fileName = "DefaultRemoteConfigValue", menuName = "Omnilatent/Default Firebase remote config value", order = 10)]
    public class FirebaseManagerConfig : ScriptableObject
    {
        private static FirebaseManagerConfig _instance;

        /// <summary>
        /// Singleton‑style access to the default remote‑config table.
        /// ‑ If the asset is already loaded, simply return it.
        /// ‑ If it is not loaded, try to <see cref="Resources.Load{T}(string)"/> it from the Resources folder.
        /// ‑ If it still cannot be found *and* we are in the Editor, create one at
        ///   <c>Assets/Resources/DefaultRemoteConfigValue.asset</c> so future loads succeed.
        /// </summary>
        public static FirebaseManagerConfig Instance
        {
            get
            {
                if (_instance == null)
                {
                    // Attempt to load from Resources.
                    _instance = Resources.Load<FirebaseManagerConfig>("FirebaseManagerConfig");

#if UNITY_EDITOR
                    // If not found, create a new asset inside the Resources folder (Editor‑only).
                    if (_instance == null)
                    {
                        const string assetPath = "Assets/Omnilatent/Extra/FirebaseManager Extra/Resources/FirebaseManagerConfig.asset";

                        // Ensure the Resources directory exists.
                        string directory = Path.GetDirectoryName(assetPath);
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        _instance = CreateInstance<FirebaseManagerConfig>();
                        AssetDatabase.CreateAsset(_instance, assetPath);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                        Debug.Log("Generated default FirebaseManagerConfig asset at " + assetPath);
                    }
#endif
                }

                return _instance;
            }
        }

        [Tooltip("Cached Remote Config default values")]
        public List<CacheConfigValue> configValues = new List<CacheConfigValue>();
        
        [Tooltip("First scene's name. Used by Test script to test initialization")]
        public string MainSceneName = "Main";
    }
}
