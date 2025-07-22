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
        
        public SerializedBlackboardEntry(string key, string type, string value)
        {
            this.key = key;
            this.type = type;
            this.value = value;
        }
        
        public bool TryGetValue<T>(out T outValue)
        {
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
            }
            else
            {
                _runtimeValue = new RuntimeValue<T>(newValue);
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
                
                var wrapperType = typeof(SerializableWrapper<>).MakeGenericType(valueType);
                var wrapperInstance = JsonUtility.FromJson(entry.Value, wrapperType);

                if (wrapperInstance == null)
                {
                    Debug.LogWarning($"Failed to deserialize value for key '{entry.Key}'. Skipping entry.");
                    continue;
                }

                var valueField = wrapperType.GetField("value");
                var value = valueField?.GetValue(wrapperInstance);
                var runtimeValueType = typeof(RuntimeValue<>).MakeGenericType(valueType);
                var runtimeValueInstance = Activator.CreateInstance(runtimeValueType, value);
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

        private void SetValue_Editor<T>(string key, T value)
        {
            var serializedValue = JsonUtility.ToJson(new SerializableWrapper<T> { value = value });
            var typeName = typeof(T).AssemblyQualifiedName;

            if (_entries.ContainsKey(key))
            {
                _entries[key].Type = typeName;
                _entries[key].Value = serializedValue;
            }
            else
            {
                var newEntry = new SerializedBlackboardEntry(key, typeName, serializedValue);
                _entries[key] = newEntry;
            }
        }

        private void SetValue_Runtime<T>(string key, T value)
        {
            if (!_entries.ContainsKey(key))
            {
                var newEntry = new SerializedBlackboardEntry(key, typeof(T).AssemblyQualifiedName, string.Empty);
                newEntry.SetValueRuntime(value);
                _entries[key] = newEntry;
            }
            else
            {
                _entries[key].SetValueRuntime(value);
            }
        }

        private T GetValue_Editor<T>(string key, T defaultValue = default)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                return defaultValue;
            }
            
            return entry.TryGetValue<T>(out var serializedValue) ? serializedValue : defaultValue;
        }

        private T GetValue_Runtime<T>(string key, T defaultValue = default)
        {
            if (!_entries.TryGetValue(key, out var entry))
            {
                return defaultValue;
            }

            if (entry.TryGetValueRuntime<T>(out var runtimeValue))
            {
                return runtimeValue;
            }
            
            return entry.TryGetValue<T>(out var serializedValue) ? serializedValue : defaultValue;
        }
    }
}
