using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if !OMNILATENT_FIREBASE_MANAGER
namespace Omnilatent.FirebaseNS.Editor
{
    public static class ScriptingDefineSymbolAdsManager
    {
        const string SYMBOL = "OMNILATENT_FIREBASE_MANAGER";

        [InitializeOnLoadMethod]
        private static void Init()
        {
            // Get current defines
            string defineSymbolString = PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup);
            // Split at ;
            List<string> symbols = defineSymbolString.Split(';').ToList();
            // check if defines already exist given define
            if (!symbols.Contains(SYMBOL))
            {
                // if not add it at the end with a leading ; separator
                defineSymbolString += $";{SYMBOL}";

                // write the new defines back to the PlayerSettings
                // This will cause a recompilation of your scripts
                PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defineSymbolString);

                Debug.Log($"Scripting Define Symbol '{SYMBOL}' was added.");
            }
        }
    }
}
#endif