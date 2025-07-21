using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    [System.Serializable]
    public class Blackboard
    {
        [SerializeField] private Dictionary<string, object> _data = new();
        
        // 事件，当黑板数据变化时触发
        public event Action<string, object> OnValueChanged;

        // 设置值
        public void SetValue<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
                return;

            bool hasChanged = !_data.ContainsKey(key) || !Equals(_data[key], value);
            
            _data[key] = value;

            if (hasChanged)
            {
                OnValueChanged?.Invoke(key, value);
            }
        }

        // 获取值
        public T GetValue<T>(string key)
        {
            if (string.IsNullOrEmpty(key) || !_data.ContainsKey(key))
                return default(T);

            try
            {
                return (T)_data[key];
            }
            catch (InvalidCastException)
            {
                Debug.LogWarning($"Cannot cast blackboard value '{key}' to type {typeof(T)}");
                return default(T);
            }
        }

        // 获取值，如果不存在则返回默认值
        public T GetValue<T>(string key, T defaultValue)
        {
            if (string.IsNullOrEmpty(key) || !_data.ContainsKey(key))
                return defaultValue;

            try
            {
                return (T)_data[key];
            }
            catch (InvalidCastException)
            {
                Debug.LogWarning($"Cannot cast blackboard value '{key}' to type {typeof(T)}");
                return defaultValue;
            }
        }

        // 检查是否包含指定键
        public bool HasKey(string key)
        {
            return !string.IsNullOrEmpty(key) && _data.ContainsKey(key);
        }

        // 移除键值对
        public bool RemoveKey(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            bool removed = _data.Remove(key);
            if (removed)
                OnValueChanged?.Invoke(key, null);
                
            return removed;
        }

        // 清空所有数据
        public void Clear()
        {
            var keys = new List<string>(_data.Keys);
            _data.Clear();
            
            foreach (var key in keys)
            {
                OnValueChanged?.Invoke(key, null);
            }
        }

        // 获取所有键
        public IEnumerable<string> GetAllKeys()
        {
            return _data.Keys;
        }

        // 获取指定类型的所有值
        public Dictionary<string, T> GetAllValues<T>()
        {
            var result = new Dictionary<string, T>();
            
            foreach (var kvp in _data)
            {
                if (kvp.Value is T value)
                {
                    result[kvp.Key] = value;
                }
            }
            
            return result;
        }

        // 复制黑板数据
        public Blackboard Clone()
        {
            var clone = new Blackboard();
            foreach (var kvp in _data)
            {
                clone._data[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        // 合并另一个黑板的数据
        public void Merge(Blackboard other, bool overrideExisting = true)
        {
            if (other == null)
                return;

            foreach (var kvp in other._data)
            {
                if (overrideExisting || !_data.ContainsKey(kvp.Key))
                {
                    SetValue(kvp.Key, kvp.Value);
                }
            }
        }
    }
}
