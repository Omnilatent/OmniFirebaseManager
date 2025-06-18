using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Omnilatent.FirebaseNS.Editor
{
    public class InitialSetup : EditorWindow
    {
        private static InitialSetup _instance;
        private const string PackageName = "Firebase Manager";
        private const string SYMBOL = "OMNILATENT_FIREBASE_MANAGER";
        private const string DATABUCKET_SYMBOL = "JACAT_DATABUCKET";
        private const string DATABUCKET_REALTIME_SYMBOL = "DATABUCKET_REALTIME";

        #if !OMNILATENT_FIREBASE_MANAGER
        [UnityEditor.Callbacks.DidReloadScripts]
        #endif
        private static void ShowInstallWindowWhenReady()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += ShowInstallWindowWhenReady;
                return;
            }

            EditorApplication.delayCall += ShowInstallWindow;
        }
        
        [MenuItem("Tools/Omnilatent/Firebase/Import essential files")]
        public static void ShowInstallWindow()
        {
            if (_instance == null)
            {
                _instance = GetWindow<InitialSetup>();
                _instance.maxSize = new Vector2(670f, 280f);
                _instance.minSize = new Vector2(500f, 280f);
                _instance.titleContent = new GUIContent($"{PackageName} Initial Setup");
            }
            else
            {
                _instance.Focus();
            }

            #if !OMNILATENT_FIREBASE_MANAGER
            ScriptingDefineSymbolAdsManager.Init();
            #endif
        }

        public void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            Label label = new Label($"Click on the following button to import files needed for {PackageName} to work properly.");
            label.style.marginTop = label.style.marginBottom = 20;
            label.style.marginLeft = label.style.marginRight = 20;
            label.style.alignSelf = new StyleEnum<Align>(Align.Center);
            label.style.whiteSpace = WhiteSpace.Normal;
            root.Add(label);

            Button button = new Button();
            button.style.height = 80;
            button.style.marginTop = new StyleLength(StyleKeyword.Auto);
            button.style.marginBottom = 10;
            button.name = "button";
            button.text = $"Import {PackageName} Extra Files";
            button.clicked += InstallFiles;
            root.Add(button);
            
            Button dataBucketButton = new Button();
            dataBucketButton.style.height = 80;
            dataBucketButton.style.marginTop = new StyleLength(StyleKeyword.Auto);
            dataBucketButton.style.marginBottom = 10;
            dataBucketButton.text = $"Import DataBucket";
            dataBucketButton.clicked += InitDataBucketScriptingDefineSymbol;
            root.Add(dataBucketButton);

            Button dataBucketRealTimeButton = new Button();
            dataBucketRealTimeButton.style.height = 80;
            dataBucketRealTimeButton.style.marginTop = new StyleLength(StyleKeyword.Auto);
            dataBucketRealTimeButton.style.marginBottom = 10;

            bool symbolExists = ScriptingDefineUtility.HasSymbol(DATABUCKET_REALTIME_SYMBOL);
            dataBucketRealTimeButton.text = symbolExists ? "Disable DataBucket Real Time Logging" : "Enable DataBucket Real Time Logging";

            dataBucketRealTimeButton.clicked += () =>
            {
                ScriptingDefineUtility.ToggleSymbol(DATABUCKET_REALTIME_SYMBOL);
                dataBucketRealTimeButton.text = ScriptingDefineUtility.HasSymbol(DATABUCKET_REALTIME_SYMBOL)
                    ? "Disable DataBucket Real Time Logging"
                    : "Enable DataBucket Real Time Logging";
            };

            root.Add(dataBucketRealTimeButton);
        }

        private void InstallFiles()
        {
            SetupFirebase.ImportCloudMessagingHelper();
        }
        
        public static void InitDataBucketScriptingDefineSymbol()
        {
            #if !JACAT_DATABUCKET
            string defineSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> symbols = defineSymbolString.Split(';').ToList();
            if (!symbols.Contains(DATABUCKET_SYMBOL))
            {
                defineSymbolString += $";{DATABUCKET_SYMBOL}";
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbolString);
                Debug.Log($"Scripting Define Symbol '{DATABUCKET_SYMBOL}' was added.");
            }
            #endif
        }
    }
    
    public static class ScriptingDefineUtility
    {
        public static bool HasSymbol(string symbol)
        {
            string defineSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            List<string> symbols = defineSymbolString.Split(';').ToList();
            return symbols.Contains(symbol);
        }

        public static void ToggleSymbol(string symbol)
        {
            BuildTargetGroup group = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defineSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
            List<string> symbols = defineSymbolString.Split(';').ToList();

            if (symbols.Contains(symbol))
            {
                symbols.Remove(symbol);
                Debug.Log($"Scripting Define Symbol '{symbol}' was removed.");
            }
            else
            {
                symbols.Add(symbol);
                Debug.Log($"Scripting Define Symbol '{symbol}' was added.");
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(group, string.Join(";", symbols));
        }
    }
}
