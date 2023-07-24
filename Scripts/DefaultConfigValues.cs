using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.FirebaseManagerNS
{
    [CreateAssetMenu(fileName = "DefaultRemoteConfigValue", menuName = "Omnilatent/Default Firebase remote config value", order = 10)]
    public class DefaultConfigValue : ScriptableObject
    {
        public List<CacheConfigValue> configValues = new List<CacheConfigValue>();
    }
}