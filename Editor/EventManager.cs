/*
 * Author: MartinYeung
 * Created On: 2025.07.06
 * Description: EventManager is a singleton class that manages events in Unity.
 */
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace YCFramework.EventManager
{
    public class EventManager : MonoBehaviour
    {
        private static EventManager instance;

        public static EventManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new EventManager();
                }

                return instance;
            }
        }

        // 普通事件字典
        private Dictionary<string, Delegate> m_EventDict = new();

        // 弱事件字典
        private Dictionary<string, ConditionalWeakTable<object, Delegate>> m_WeakEventDict = new();

        #region 添加监听

        public void AddListener(string eventName, Action callback)
        {
            OnListenerAdding(eventName, callback);
            m_EventDict[eventName] = (Action)m_EventDict[eventName] + callback;
        }

        // 添加弱事件监听
        public void AddWeakListener(string eventName, Action callback, object target)
        {
            if (!m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                table = new ConditionalWeakTable<object, Delegate>();
                m_WeakEventDict[eventName] = table;
            }

            if (table.TryGetValue(target, out var existingDelegate))
            {
                table.Remove(target);
                table.Add(target, Delegate.Combine(existingDelegate, callback));
            }
            else
            {
                table.Add(target, callback);
            }
        }

        public void AddListener<T>(string eventName, Action<T> callback)
        {
            OnListenerAdding(eventName, callback);
            m_EventDict[eventName] = (Action<T>)m_EventDict[eventName] + callback;
        }

        // 添加泛型弱事件监听
        public void AddWeakListener<T>(string eventName, Action<T> callback, object target)
        {
            if (!m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                table = new ConditionalWeakTable<object, Delegate>();
                m_WeakEventDict[eventName] = table;
            }

            if (table.TryGetValue(target, out var existingDelegate))
            {
                table.Remove(target);
                table.Add(target, Delegate.Combine(existingDelegate, callback));
            }
            else
            {
                table.Add(target, callback);
            }
        }

        public void AddListener<T1, T2>(string eventName, Action<T1, T2> callback)
        {
            OnListenerAdding(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2>)m_EventDict[eventName] + callback;
        }

        public void AddListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
        {
            OnListenerAdding(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2, T3>)m_EventDict[eventName] + callback;
        }

        public void AddListener<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> callback)
        {
            OnListenerAdding(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2, T3, T4>)m_EventDict[eventName] + callback;
        }

        #endregion

        #region 移除监听

        public void RemoveListener(string eventName, Action callback)
        {
            OnListenerRemoving(eventName, callback);
            m_EventDict[eventName] = (Action)m_EventDict[eventName] - callback;
            OnListenerRemoved(eventName);
        }

        // 移除弱事件监听
        public void RemoveWeakListener(string eventName, object target)
        {
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                table.Remove(target);

                // 如果该事件的所有弱引用都被移除，则移除整个事件
                var hasEntries = false;
                foreach (var _ in table)
                {
                    hasEntries = true;
                    break;
                }

                if (!hasEntries)
                {
                    m_WeakEventDict.Remove(eventName);
                }
            }
        }

        public void RemoveListener<T>(string eventName, Action<T> callback)
        {
            OnListenerRemoving(eventName, callback);
            m_EventDict[eventName] = (Action<T>)m_EventDict[eventName] - callback;
            OnListenerRemoved(eventName);
        }

        public void RemoveListener<T1, T2>(string eventName, Action<T1, T2> callback)
        {
            OnListenerRemoving(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2>)m_EventDict[eventName] - callback;
            OnListenerRemoved(eventName);
        }

        public void RemoveListener<T1, T2, T3>(string eventName, Action<T1, T2, T3> callback)
        {
            OnListenerRemoving(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2, T3>)m_EventDict[eventName] - callback;
            OnListenerRemoved(eventName);
        }

        public void RemoveListener<T1, T2, T3, T4>(string eventName, Action<T1, T2, T3, T4> callback)
        {
            OnListenerRemoving(eventName, callback);
            m_EventDict[eventName] = (Action<T1, T2, T3, T4>)m_EventDict[eventName] - callback;
            OnListenerRemoved(eventName);
        }

        #endregion

        #region 触发事件

        public void TriggerEvent(string eventName)
        {
            // 触发普通事件
            if (m_EventDict.TryGetValue(eventName, out var d))
            {
                if (d is Action action)
                {
                    action();
                }
            }

            // 触发弱事件
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                var deadTargets = new List<object>();

                foreach (var pair in table)
                {
                    if (pair.Value is Action action)
                    {
                        try
                        {
                            action();
                        }
                        catch (Exception)
                        {
                            // 如果调用失败，可能是目标已经被销毁，将其加入待清理列表
                            deadTargets.Add(pair.Key);
                        }
                    }
                }

                // 清理已失效的弱引用
                foreach (var target in deadTargets)
                {
                    table.Remove(target);
                }
            }
        }

        public void TriggerEvent<T>(string eventName, T arg)
        {
            // 触发普通事件
            if (m_EventDict.TryGetValue(eventName, out var d))
            {
                if (d is Action<T> action)
                {
                    action(arg);
                }
            }

            // 触发弱事件
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                var deadTargets = new List<object>();

                foreach (var pair in table)
                {
                    if (pair.Value is Action<T> action)
                    {
                        try
                        {
                            action(arg);
                        }
                        catch (Exception)
                        {
                            deadTargets.Add(pair.Key);
                        }
                    }
                }

                foreach (var target in deadTargets)
                {
                    table.Remove(target);
                }
            }
        }

        public void TriggerEvent<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            if (m_EventDict.TryGetValue(eventName, out var d))
            {
                if (d is Action<T1, T2> action)
                {
                    action(arg1, arg2);
                }
            }

            // 触发弱事件
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                var deadTargets = new List<object>();

                foreach (var pair in table)
                {
                    if (pair.Value is Action<T1, T2> action)
                    {
                        try
                        {
                            action(arg1, arg2);
                        }
                        catch (Exception)
                        {
                            deadTargets.Add(pair.Key);
                        }
                    }
                }

                foreach (var target in deadTargets)
                {
                    table.Remove(target);
                }
            }
        }

        public void TriggerEvent<T1, T2, T3>(string eventName, T1 arg1, T2 arg2, T3 arg3)
        {
            if (m_EventDict.TryGetValue(eventName, out var d))
            {
                if (d is Action<T1, T2, T3> action)
                {
                    action(arg1, arg2, arg3);
                }
            }

            // 触发弱事件
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                var deadTargets = new List<object>();

                foreach (var pair in table)
                {
                    if (pair.Value is Action<T1, T2, T3> action)
                    {
                        try
                        {
                            action(arg1, arg2, arg3);
                        }
                        catch (Exception)
                        {
                            deadTargets.Add(pair.Key);
                        }
                    }
                }

                foreach (var target in deadTargets)
                {
                    table.Remove(target);
                }
            }
        }

        public void TriggerEvent<T1, T2, T3, T4>(string eventName, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
        {
            if (m_EventDict.TryGetValue(eventName, out var d))
            {
                if (d is Action<T1, T2, T3, T4> action)
                {
                    action(arg1, arg2, arg3, arg4);
                }
            }

            // 触发弱事件
            if (m_WeakEventDict.TryGetValue(eventName, out var table))
            {
                var deadTargets = new List<object>();

                foreach (var pair in table)
                {
                    if (pair.Value is Action<T1, T2, T3, T4> action)
                    {
                        try
                        {
                            action(arg1, arg2, arg3, arg4);
                        }
                        catch (Exception)
                        {
                            deadTargets.Add(pair.Key);
                        }
                    }
                }

                foreach (var target in deadTargets)
                {
                    table.Remove(target);
                }
            }
        }

        #endregion

        #region 辅助方法

        private void OnListenerAdding(string eventName, Delegate callback)
        {
            if (!m_EventDict.ContainsKey(eventName))
            {
                m_EventDict.Add(eventName, null);
            }

            var d = m_EventDict[eventName];
            if (d != null && d.GetType() != callback.GetType())
            {
                throw new Exception($"尝试为事件 {eventName} 添加不同类型的委托，" +
                                    $"当前事件所对应的委托为 {d.GetType()}，要添加的委托类型为 {callback.GetType()}");
            }
        }

        private void OnListenerRemoving(string eventName, Delegate callback)
        {
            if (m_EventDict.ContainsKey(eventName))
            {
                var d = m_EventDict[eventName];
                if (d == null)
                {
                    throw new Exception($"移除监听错误：事件 {eventName} 没有委托");
                }
                else if (d.GetType() != callback.GetType())
                {
                    throw new Exception($"移除监听错误：尝试为事件 {eventName} 移除不同类型的委托，" +
                                        $"当前事件所对应的委托为 {d.GetType()}，要移除的委托类型为 {callback.GetType()}");
                }
            }
            else
            {
                throw new Exception($"移除监听错误：没有事件名为 {eventName} 的事件");
            }
        }

        private void OnListenerRemoved(string eventName)
        {
            if (m_EventDict[eventName] == null)
            {
                m_EventDict.Remove(eventName);
            }
        }

        /// <summary>
        /// 清除所有事件
        /// </summary>
        public void ClearAllEvents()
        {
            m_EventDict.Clear();
            m_WeakEventDict.Clear();
        }

        /// <summary>
        /// 清理失效的弱引用
        /// </summary>
        public void CleanupDeadWeakReferences()
        {
            var emptyEvents = new List<string>();

            foreach (var kvp in m_WeakEventDict)
            {
                var hasValidEntries = false;
                var deadTargets = new List<object>();

                foreach (var pair in kvp.Value)
                {
                    try
                    {
                        if (pair.Value != null)
                        {
                            hasValidEntries = true;
                        }
                        else
                        {
                            deadTargets.Add(pair.Key);
                        }
                    }
                    catch
                    {
                        deadTargets.Add(pair.Key);
                    }
                }

                foreach (var target in deadTargets)
                {
                    kvp.Value.Remove(target);
                }

                if (!hasValidEntries)
                {
                    emptyEvents.Add(kvp.Key);
                }
            }

            foreach (var eventName in emptyEvents)
            {
                m_WeakEventDict.Remove(eventName);
            }
        }

        #endregion
    }
}
