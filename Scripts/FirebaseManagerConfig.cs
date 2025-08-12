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
        [Tooltip("Cached Remote Config default values")]
        public List<CacheConfigValue> configValues = new List<CacheConfigValue>();

        [Tooltip("First scene's name. Used by Test script to test initialization")]
        public string MainSceneName = "Main";

        private static FirebaseManagerConfig _instance;
        private const string _assetName = "FirebaseManagerConfig";
        private const string _assetNameLegacy = "DefaultRemoteConfigValue";
        const string _assetFolder = "Assets/Omnilatent/Extra/FirebaseManager Extra/Resources";

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
                    _instance = EnsureInstance();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Ensure that a valid asset exists in Resources and migrate data from any legacy asset if needed.
        /// </summary>
        private static FirebaseManagerConfig EnsureInstance()
        {
            // 1. Strong‑typed load first
            var asset = Resources.Load<FirebaseManagerConfig>(_assetName);
            if (asset != null)
                return asset;

            #if UNITY_EDITOR
            // 2. Look for any ScriptableObject with the legacy name for migration
            var legacyAsset = Resources.Load<FirebaseManagerConfig>(_assetNameLegacy);

            // 3. Create new asset
            var newAsset = CreateInstance<FirebaseManagerConfig>();

            if (legacyAsset != null && legacyAsset != newAsset)
            {
                MigrateLegacyValues(legacyAsset, newAsset);
                Debug.Log($"[FirebaseManagerConfig] Migrated values from legacy asset {_assetNameLegacy}.");
            }

            // 4. Save into Assets/Resources so runtime can load it next time
            var resourcesDir = _assetFolder;
            EnsureFolderExists(resourcesDir);

            var assetPath = Path.Combine(resourcesDir, _assetName + ".asset");
            AssetDatabase.CreateAsset(newAsset, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return newAsset;
            #else
            Debug.LogError("[FirebaseManagerConfig] Asset not found in Resources and cannot be created at runtime.");
            return null;
            #endif
        }

        /// <summary>
        /// Copy matching public fields and properties from a legacy asset to the new asset.
        /// </summary>
        private static void MigrateLegacyValues(FirebaseManagerConfig source, FirebaseManagerConfig destination)
        {
            destination.configValues = new(source.configValues);
        }

        #if UNITY_EDITOR
        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath)) return;

            string[] parts = folderPath.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = $"{current}/{parts[i]}";
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
        #endif
    }
}