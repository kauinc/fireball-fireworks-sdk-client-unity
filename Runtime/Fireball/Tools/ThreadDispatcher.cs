using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fireball.Game.Client.Tools
{
    public class ThreadDispatcher : IDisposable
    {
        private static MonoBehaviour _mono = null;
        private static List<Action> _mainThreadActions = new List<Action>();

        public ThreadDispatcher(MonoBehaviour mono)
        {
            _mono = mono;
            _mono.StartCoroutine(Update());
        }

        public void InvokeInMainThread(Action action)
        {
            if (_mainThreadActions == null) _mainThreadActions = new List<Action>();
            _mainThreadActions.Add(action);
        }

        private IEnumerator Update()
        {
            while (_mono.gameObject)
            {
                if (_mainThreadActions != null && _mainThreadActions.Count > 0)
                {
                    var _cache = new List<Action>(_mainThreadActions);
                    foreach (var action in _cache)
                    {
                        action.Invoke();
                    }
                    _mainThreadActions.Clear();
                }
                yield return null;
            }
        }

        public void Dispose()
        {
            _mono = null;
            _mainThreadActions = new List<Action>();
        }
    }
}
