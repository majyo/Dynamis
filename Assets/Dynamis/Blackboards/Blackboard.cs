using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace Dynamis.Blackboards
{
    [Serializable]
    public class BlackboardEntry
    {
        [SerializeField] private string key;
        [SerializeField] private string type;
        [SerializeField] private string value;
        
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
        
        public BlackboardEntry(string key, string type, string value)
        {
            this.key = key;
            this.type = type;
            this.value = value;
        }
    }

    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Dynamis/Blackboard")]
    public class Blackboard : ScriptableObject
    {
        [SerializeField] private List<BlackboardEntry> _entries = new();
        private readonly Dictionary<string, BlackboardEntry> _entryCache = new();
        
        public List<BlackboardEntry> Entries => _entries;
        
        private void OnEnable()
        {
            RebuildCache();
        }
        
        private void RebuildCache()
        {
            _entryCache.Clear();
            
            foreach (var entry in _entries)
            {
                if (!string.IsNullOrEmpty(entry.Key))
                {
                    _entryCache[entry.Key] = entry;
                }
            }
        }
        
        public void SetValue<T>(string key, T value)
        {
            string serializedValue = JsonUtility.ToJson(new SerializableWrapper<T> { value = value });
            string typeName = typeof(T).AssemblyQualifiedName;
            
            if (_entryCache.ContainsKey(key))
            {
                _entryCache[key].Type = typeName;
                _entryCache[key].Value = serializedValue;
            }
            else
            {
                var newEntry = new BlackboardEntry(key, typeName, serializedValue);
                _entries.Add(newEntry);
                _entryCache[key] = newEntry;
            }
        }
        
        public T GetValue<T>(string key, T defaultValue = default(T))
        {
            if (_entryCache.ContainsKey(key))
            {
                try
                {
                    var wrapper = JsonUtility.FromJson<SerializableWrapper<T>>(_entryCache[key].Value);
                    return wrapper.value;
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Failed to deserialize value for key '{key}': {ex.Message}");
                }
            }
            
            return defaultValue;
        }
        
        public bool HasKey(string key)
        {
            return _entryCache.ContainsKey(key);
        }
        
        public bool RemoveKey(string key)
        {
            if (_entryCache.ContainsKey(key))
            {
                var entry = _entryCache[key];
                _entries.Remove(entry);
                _entryCache.Remove(key);
                return true;
            }
            
            return false;
        }
        
        public void Clear()
        {
            _entries.Clear();
            _entryCache.Clear();
        }
        
        public string[] GetAllKeys()
        {
            var keys = new string[_entryCache.Count];
            _entryCache.Keys.CopyTo(keys, 0);
            return keys;
        }
        
        // 用于序列化任意类型的包装器
        [Serializable]
        private class SerializableWrapper<T>
        {
            public T value;
        }
        
        // Editor相关方法
        public void AddEntry(string key, string type, string value)
        {
            if (string.IsNullOrEmpty(key)) return;
            
            if (_entryCache.ContainsKey(key))
            {
                _entryCache[key].Type = type;
                _entryCache[key].Value = value;
            }
            else
            {
                var newEntry = new BlackboardEntry(key, type, value);
                _entries.Add(newEntry);
                _entryCache[key] = newEntry;
            }
        }
        
        public void RemoveEntryAt(int index)
        {
            if (index >= 0 && index < _entries.Count)
            {
                var entry = _entries[index];
                if (!string.IsNullOrEmpty(entry.Key) && _entryCache.ContainsKey(entry.Key))
                {
                    _entryCache.Remove(entry.Key);
                }
                _entries.RemoveAt(index);
            }
        }
        
        public void ValidateEntries()
        {
            RebuildCache();
        }
    }
}
