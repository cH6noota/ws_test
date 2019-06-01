using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using System.Linq;

namespace WebSocket4Unity
{
    public class Interaction : MonoBehaviour
    {
        /// <summary>
        /// 定义最大线程数
        /// </summary>
        private static int maxThreads = 8;
        /// <summary>
        /// 同时运行几个线程的数量
        /// </summary>
        private static int numThreads;
        public class QueueItem
        {
            public Action<object, object> action { get; set; }
            public object param1 { get; set; }
            public object param2 { get; set; }
        }
        public class DelayedQueueItem : QueueItem
        {
            public float time { get; set; }
        }
        private readonly List<QueueItem> _actions = new List<QueueItem>();
        private readonly List<QueueItem> _currentActions = new List<QueueItem>();
        private readonly List<DelayedQueueItem> _delayed = new List<DelayedQueueItem>();
        private readonly List<DelayedQueueItem> _currentDelayed = new List<DelayedQueueItem>();
        /// <summary>
        /// 单例
        /// </summary>
        private static Interaction _current;
        /// <summary>
        /// 标记是否实例化
        /// </summary>
        private static bool initialized;
        public static Interaction Current
        {
            get
            {
                Initialize();
                return _current;
            }
        }
        public static void Initialize()
        {
            if (!initialized)
            {
                if (!Application.isPlaying)
                {
                    return;
                }
                initialized = true;
                _current = new GameObject("Interaction").AddComponent<Interaction>();
            }
        }
        public static void QueueOnMainThread(Action<object, object> _action, object _param1, object _param2)
        {
            QueueOnMainThread(_action, _param1, _param2, 0f);
        }
        public static void QueueOnMainThread(Action<object, object> _action, object _param1, object _param2, float _time)
        {
            if (_time != 0)
            {
                lock (Current._delayed)
                {
                    Current._delayed.Add(new DelayedQueueItem { time = Time.time + _time, action = _action, param1 = _param1, param2 = _param2 });
                }
            }
            else
            {
                lock (Current._actions)
                {
                    Current._actions.Add(new QueueItem { action = _action, param1 = _param1, param2 = _param2 });
                }
            }
        }
        public static Thread RunAsync(Action a)
        {
            Initialize();
            while (numThreads >= maxThreads)
            {
                Thread.Sleep(1);
            }
            Interlocked.Increment(ref numThreads);
            ThreadPool.QueueUserWorkItem(RunAction, a);
            return null;
        }
        private static void RunAction(object action)
        {
            try
            {
                ((Action)action)();
            }
            catch (System.Exception ex)
            {
                Debug.Log(ex.Message);
            }
            finally
            {
                Interlocked.Decrement(ref numThreads);
            }
        }
        private void OnDisable()
        {
            if (_current == this)
            {
                _current = null;
            }
        }
        private void Update()
        {
            lock (_actions)
            {
                _currentActions.Clear();
                _currentActions.AddRange(_actions);
                _actions.Clear();
            }
            for (int i = 0; i < _currentActions.Count; i++)
            {
                _currentActions[i].action(_currentActions[i].param1, _currentActions[i].param2);
            }
            lock (_delayed)
            {
                _currentDelayed.Clear();
                _currentDelayed.AddRange(_delayed.Where(d => d.time <= Time.time));
                for (int i = 0; i < _currentDelayed.Count; i++)
                {
                    _delayed.Remove(_currentDelayed[i]);
                }
            }
            for (int i = 0; i < _currentDelayed.Count; i++)
            {
                _currentDelayed[i].action(_currentDelayed[i].param1, _currentDelayed[i].param2);
            }
        }
    }
}
