using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Omnilatent.FirebaseNS.Editor
{
    public class SetupFirebase
    {
        [MenuItem("Tools/Omnilatent/Firebase/Import Extra files")]
        public static void ImportCloudMessagingHelper()
        {
            string path = GetPackagePath("Assets/Omnilatent/FirebaseManager/FirebaseManagerExtra.unitypackage", "FirebaseManagerExtra");
            AssetDatabase.ImportPackage(path, true);
        }

        static string GetPackagePath(string path, string filename)
        {
            if (!File.Exists($"{Application.dataPath}/../{path}"))
            {
                Debug.Log($"{filename} not found at {path}, attempting to search whole project for {filename}");
                string[] guids = AssetDatabase.FindAssets($"{filename} l:package");
                if (guids.Length > 0)
                {
                    path = AssetDatabase.GUIDToAssetPath(guids[0]);
                }
                else
                {
                    Debug.LogError($"{filename} not found at {Application.dataPath}/../{path}");
                    return null;
                }
            }
            return path;
        }
    }
}