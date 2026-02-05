using System;
using System.Collections.Generic;
using UnityEngine;

namespace Omnilatent.FirebaseManagerNS
{
    public class MainThreadDispatcher : MonoBehaviour
    {
        private static readonly Queue<Action> _executionQueue = new Queue<Action>();

        private static MainThreadDispatcher _instance;

        public static MainThreadDispatcher Instance
        {
            get
            {
                if (!_instance)
                {
                    _instance = FindObjectOfType<MainThreadDispatcher>();

                    if (!_instance)
                    {
                        var obj = new GameObject("ThreadDispatcher");
                        _instance = obj.AddComponent<MainThreadDispatcher>();
                        DontDestroyOnLoad(obj);
                    }
                }

                return _instance;
            }
        }

        public static void ExecuteOnMainThread(Action action)
        {
            if (action == null)
                throw new ArgumentNullException(nameof(action));

            lock (_executionQueue)
            {
                _executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            while (_executionQueue.Count > 0)
            {
                Action action;
                lock (_executionQueue)
                {
                    action = _executionQueue.Dequeue();
                }

                action?.Invoke();
            }
        }
    }
}