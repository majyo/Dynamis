using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Scripts.Behaviours
{
    /// <summary>
    /// 黑板系统 - 用于在行为树节点之间共享数据
    /// </summary>
    public class Blackboard : MonoBehaviour
    {
        private readonly Dictionary<string, object> _data = new();

        /// <summary>
        /// 设置值
        /// </summary>
        public void SetValue<T>(string key, T value)
        {
            _data[key] = value;
        }

        /// <summary>
        /// 获取值
        /// </summary>
        public T GetValue<T>(string key)
        {
            if (_data.TryGetValue(key, out object value))
            {
                if (value is T)
                {
                    return (T)value;
                }
                
                // 尝试转换
                try
                {
                    return (T)System.Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    Debug.LogWarning($"Cannot convert value of key '{key}' to type {typeof(T)}");
                    return default(T);
                }
            }
            
            return default(T);
        }

        /// <summary>
        /// 获取值，如果不存在则返回默认值
        /// </summary>
        public T GetValue<T>(string key, T defaultValue)
        {
            if (HasKey(key))
            {
                return GetValue<T>(key);
            }
            return defaultValue;
        }

        /// <summary>
        /// 检查是否存在指定键
        /// </summary>
        public bool HasKey(string key)
        {
            return _data.ContainsKey(key);
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool RemoveKey(string key)
        {
            return _data.Remove(key);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _data.Clear();
        }

        /// <summary>
        /// 获取所有键
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return _data.Keys;
        }

        /// <summary>
        /// 获取数据数量
        /// </summary>
        public int Count => _data.Count;
    }
}
