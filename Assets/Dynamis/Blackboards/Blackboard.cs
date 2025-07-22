using System;
using System.Collections.Generic;
using Frameworks.Structures.SerializableDictionary;
using UnityEngine;

namespace Dynamis.Blackboards
{
    public class RuntimeValue<T>
    {
        public Type Type { get; }
        public T Value { get; set; }
        
        public RuntimeValue(T value)
        {
            Value = value;
            Type = typeof(T);
        }
    }
    
    // 用于序列化任意类型的包装器
    [Serializable]
    public class SerializableWrapper<T>
    {
        public T value;
    }
    
    [Serializable]
    public class SerializedBlackboardEntry
    {
        [SerializeField] private string key;
        [SerializeField] private string type;
        [SerializeField] private string value;
        // 添加对UnityEngine.Object类型的支持
        [SerializeField] private UnityEngine.Object objectReference;
        
        private object _runtimeValue;
        
        public string Key 
        { 
            get => key; 
            set => key = value; 
        }
        
        public string Type 
        { 
            get => type; 
            set => type = value; 
        }
        
        public string Value 
        { 
            get => value; 
            set => this.value = value; 
        }

        public UnityEngine.Object ObjectReference
        {
            get => objectReference;
            set => objectReference = value;
        }
        
        public SerializedBlackboardEntry(string key, string type, string value)
        {
            this.key = key;
            this.type = type;
            this.value = value;
            this.objectReference = null;
        }

        // 专门为UnityEngine.Object类型的构造函数
        public SerializedBlackboardEntry(string key, string type, UnityEngine.Object objRef)
        {
            this.key = key;
            this.type = type;
            this.value = "";
            this.objectReference = objRef;
        }
        
        public bool TryGetValue<T>(out T outValue)
        {
            // 检查是否是UnityEngine.Object类型
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                if (objectReference != null && objectReference is T obj)
                {
                    outValue = obj;
                    return true;
                }
                outValue = default;
                return false;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<SerializableWrapper<T>>(value);
                outValue = wrapper.value;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to deserialize value for key '{key}': {ex.Message}");
                outValue = default;
                return false;
            }
        }

        public bool TryGetValueRuntime<T>(out T outValue)
        {
            if (_runtimeValue is RuntimeValue<T> typedRuntimeValue)
            {
                outValue = typedRuntimeValue.Value;
                return true;
            }

            // 检查是否是UnityEngine.Object类型
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                if (objectReference != null && objectReference is T obj)
                {
                    _runtimeValue = new RuntimeValue<T>(obj);
                    outValue = obj;
                    return true;
                }
                outValue = default;
                return false;
            }

            try
            {
                var wrapper = JsonUtility.FromJson<SerializableWrapper<T>>(value);
                _runtimeValue = new RuntimeValue<T>(wrapper.value);
                outValue = wrapper.value;
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to deserialize value for key '{key}': {ex.Message}");
                outValue = default;
                return false;
            }
        }
        
        public void SetValue<T>(T newValue)
        {
            // 检查是否是UnityEngine.Object类型
            if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
            {
                objectReference = newValue as UnityEngine.Object;
                return;
            }

            try
            {
                var wrapper = new SerializableWrapper<T> { value = newValue };
                value = JsonUtility.ToJson(wrapper);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"Failed to serialize value for key '{key}': {ex.Message}");
            }
        }
        
        public void SetValueRuntime<T>(T newValue)
        {
            if (_runtimeValue is RuntimeValue<T> typedRuntimeValue)
            {
                typedRuntimeValue.Value = newValue;
                
                // 如果是UnityEngine.Object类型，同时更新objectReference
                if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
                {
                    objectReference = newValue as UnityEngine.Object;
                }
            }
            else
            {
                _runtimeValue = new RuntimeValue<T>(newValue);
                
                // 如果是UnityEngine.Object类型，同时更新objectReference
                if (typeof(UnityEngine.Object).IsAssignableFrom(typeof(T)))
                {
                    objectReference = newValue as UnityEngine.Object;
                }
            }
        }
        
        public void DirectSetRuntimeValue(object runtimeValue)
        {
            _runtimeValue = runtimeValue;
        }
    }

    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Dynamis/Blackboard")]
    public class Blackboard : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, SerializedBlackboardEntry> _entries = new();
        [SerializeField] private bool _serializeWhenEditorPlaying;
        
        public SerializableDictionary<string, SerializedBlackboardEntry> Entry => _entries;

        public void SetValue<T>(string key, T value)
        {
#if UNITY_EDITOR
            if (_serializeWhenEditorPlaying && Application.isPlaying)
            {
                SetValue_Editor(key, value);
            }
            else
            {
                SetValue_Runtime(key, value);
            }
#else
            SetValue_Runtime(key, value);
#endif
        }
        
        public T GetValue<T>(string key, T defaultValue = default)
        {
#if UNITY_EDITOR
            if (_serializeWhenEditorPlaying && Application.isPlaying)
            {
                return GetValue_Editor(key, defaultValue);
            }

            return GetValue_Runtime(key, defaultValue);
#else
            return GetValue_Runtime(key, defaultValue);
#endif
        }
        
        /// <summary>
        /// Bake all serialized values to runtime values.
        /// <remarks>
        /// This method uses json deserialization and runtime reflection to convert serialized values into runtime values.
        /// It is extremely time-consuming and should only be used during initialization or when necessary.
        /// </remarks>
        /// </summary>
        public void BakeValuesToRuntime()
        {
            foreach (var pair in _entries)
            {
                var entry = pair.Value;
                var valueType = Type.GetType(entry.Type);
                
                if (valueType == null)
                {
                    Debug.LogWarning($"Type '{entry.Type}' for key '{entry.Key}' not found. Skipping entry.");
                    continue;
                }
                
                object retrievedValue = null;

                if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                {
                    retrievedValue = entry.ObjectReference;
                }
                else
                {
                    var wrapperType = typeof(SerializableWrapper<>).MakeGenericType(valueType);
                    var wrapperInstance = JsonUtility.FromJson(entry.Value, wrapperType);

                    if (wrapperInstance == null)
                    {
                        Debug.LogWarning($"Failed to deserialize value for key '{entry.Key}'. Skipping entry.");
                        continue;
                    }

                    var valueField = wrapperType.GetField("value");
                    retrievedValue = valueField?.GetValue(wrapperInstance);
                }
                
                if (retrievedValue == null)
                {
                    Debug.LogWarning($"Retrieved value for key '{entry.Key}' is null. Skipping entry.");
                    continue;
                }

                var runtimeValueType = typeof(RuntimeValue<>).MakeGenericType(valueType);
                var runtimeValueInstance = Activator.CreateInstance(runtimeValueType, retrievedValue);
                entry.DirectSetRuntimeValue(runtimeValueInstance);
            }
        }
        
        public bool HasKey(string key)
        {
            return _entries.ContainsKey(key);
        }
        
        public bool RemoveKey(string key)
        {
            return _entries.Remove(key);
        }
        
        public void Clear()
        {
            _entries.Clear();
        }

        public int GetAllKeys(List<string> keys)
        {
            if (keys == null)
            {
                Debug.LogError("The key list for retrieving all keys is null.");
                return -1;
            }

            keys.Clear();
            
            foreach (var key in _entries.Keys)
            {
                keys.Add(key);
            }
            
            return keys.Count;
        }

        public string[] GetAllKeysCopy()
        {
            var keys = new string[_entries.Count];
            _entries.Keys.CopyTo(keys, 0);
            return keys;
        }
        
        public void AddEntry(string key, string type, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }
            
            if (_entries.ContainsKey(key))
            {
                _entries[key].Type = type;
                _entries[key].Value = value;
            }
            else
            {
                var newEntry = new SerializedBlackboardEntry(key, type, value);
                _entries[key] = newEntry;
            }
        }

        public void RemoveEntry(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                return;
            }

            _entries.Remove(key);
        }

        private void SetValue_Runtime<T>(string key, T value)
        {
            if (_entries.ContainsKey(key))
            {
                _entries[key].SetValueRuntime(value);
            }
            else
            {
                var typeName = typeof(T).AssemblyQualifiedName;
                var entry = new SerializedBlackboardEntry(key, typeName, "");
                entry.SetValueRuntime(value);
                _entries.Add(key, entry);
            }
        }

        private T GetValue_Runtime<T>(string key, T defaultValue = default)
        {
            if (_entries.ContainsKey(key) && _entries[key].TryGetValueRuntime<T>(out var value))
            {
                return value;
            }
            return defaultValue;
        }

        private void SetValue_Editor<T>(string key, T value)
        {
            if (_entries.ContainsKey(key))
            {
                _entries[key].SetValue(value);
            }
            else
            {
                var typeName = typeof(T).AssemblyQualifiedName;
                var entry = new SerializedBlackboardEntry(key, typeName, "");
                entry.SetValue(value);
                _entries.Add(key, entry);
            }
        }

        private T GetValue_Editor<T>(string key, T defaultValue = default)
        {
            if (_entries.ContainsKey(key) && _entries[key].TryGetValue<T>(out var value))
            {
                return value;
            }
            return defaultValue;
        }
    }
}
