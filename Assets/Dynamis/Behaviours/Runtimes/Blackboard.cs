using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dynamis.Behaviours.Runtimes
{
    public class Blackboard : MonoBehaviour
    {
        // 内部容器接口，避免装箱
        private interface IContainer
        {
            bool HasValue { get; }
            Type ValueType { get; }
        }
        
        private class Container<T> : IContainer
        {
            public T Value { get; set; }
            public bool HasValue { get; set; }
            public Type ValueType => typeof(T);

            public Container(T value)
            {
                Value = value;
                HasValue = true;
            }
        }
        
        private readonly Dictionary<string, IContainer> _containers = new();

        public int Count 
        { 
            get
            {
                var count = 0;
                
                foreach (var container in _containers.Values)
                {
                    if (container.HasValue)
                    {
                        count++;
                    }
                }
                
                return count;
            }
        }

        public void SetValue<T>(string key, T value)
        {
            if (_containers.TryGetValue(key, out var container))
            {
                if (container is Container<T> typedContainer)
                {
                    typedContainer.Value = value;
                    typedContainer.HasValue = true;
                    return;
                }
                
                _containers.Remove(key);
            }

            _containers[key] = new Container<T>(value);
        }

        public T GetValue<T>(string key)
        {
            if (!_containers.TryGetValue(key, out var container))
            {
                return default;
            }
            
            if (container is Container<T> { HasValue: true } typedContainer)
            {
                return typedContainer.Value;
            }

            if (!container.HasValue)
            {
                return default;
            }
            
            try
            {
                var originalValue = GetRawValue(container);
                    
                if (originalValue != null)
                {
                    return (T)Convert.ChangeType(originalValue, typeof(T));
                }
            }
            catch
            {
                Debug.LogWarning($"Cannot convert value of key '{key}' from type {container.ValueType} to type {typeof(T)}");
                return default;
            }

            return default;
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
        /// 尝试获取值，类型安全
        /// </summary>
        public bool TryGetValue<T>(string key, out T value)
        {
            if (!_containers.TryGetValue(key, out var container))
            {
                value = default;
                return false;
            }
            
            if (container is Container<T> { HasValue: true } typedContainer)
            {
                value = typedContainer.Value;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// 检查是否存在指定键
        /// </summary>
        public bool HasKey(string key)
        {
            return _containers.TryGetValue(key, out IContainer container) && container.HasValue;
        }

        /// <summary>
        /// 检查指定键是否为特定类型
        /// </summary>
        public bool HasKey<T>(string key)
        {
            return _containers.TryGetValue(key, out IContainer container) &&
                   container is Container<T> typedContainer && typedContainer.HasValue;
        }

        /// <summary>
        /// 移除键值对
        /// </summary>
        public bool RemoveKey(string key)
        {
            return _containers.Remove(key);
        }

        /// <summary>
        /// 清空所有数据
        /// </summary>
        public void Clear()
        {
            _containers.Clear();
        }

        /// <summary>
        /// 获取所有键
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            foreach (var kvp in _containers)
            {
                if (kvp.Value.HasValue)
                {
                    yield return kvp.Key;
                }
            }
        }

        /// <summary>
        /// 获取指定类型的所有键
        /// </summary>
        public IEnumerable<string> GetKeysOfType<T>()
        {
            foreach (var kvp in _containers)
            {
                if (kvp.Value is Container<T> container && container.HasValue)
                {
                    yield return kvp.Key;
                }
            }
        }

        /// <summary>
        /// 获取指定键的值类型
        /// </summary>
        public Type GetValueType(string key)
        {
            if (_containers.TryGetValue(key, out IContainer container))
            {
                return container.ValueType;
            }
            return null;
        }
        
        /// <summary>
        /// 获取原始值（用于类型转换）
        /// </summary>
        private static object GetRawValue(IContainer container)
        {
            // 使用反射获取原始值，只在类型转换时使用
            var containerType = container.GetType();
            if (containerType.IsGenericType && containerType.GetGenericTypeDefinition() == typeof(Container<>))
            {
                var valueProperty = containerType.GetProperty("Value");
                return valueProperty?.GetValue(container);
            }
            return null;
        }
    }
}
