using System;
using System.Collections.Generic;

namespace Dynamis.Behaviours.Runtimes.Blackboards
{
    [Serializable]
    public readonly struct BlackboardKey : IEquatable<BlackboardKey>
    {
        private readonly string _name;
        private readonly int _hashedKey;

        public BlackboardKey(string name)
        {
            _name = name;
            _hashedKey = name.ComputeFNV1AHash();
        }

        public bool Equals(BlackboardKey other)
        {
            return _hashedKey == other._hashedKey;
        }

        public override bool Equals(object obj)
        {
            return obj is BlackboardKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashedKey;
        }

        public override string ToString()
        {
            return _name;
        }

        public static bool operator ==(BlackboardKey left, BlackboardKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(BlackboardKey left, BlackboardKey right)
        {
            return !left.Equals(right);
        }
    }

    [Serializable]
    public class BlackboardEntry<T>
    {
        public BlackboardKey Key { get; }
        public T Value { get; set; }
        public Type ValueType { get; }

        public BlackboardEntry(BlackboardKey key, T value)
        {
            Key = key;
            Value = value;
            ValueType = typeof(T);
        }

        public override bool Equals(object obj)
        {
            return obj is BlackboardEntry<T> other && Key == other.Key;
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }
    }

    [Serializable]
    public class Blackboard : IBlackboard
    {
        private Dictionary<string, BlackboardKey> _keyRegistry = new();
        private Dictionary<BlackboardKey, object> _entries = new();

        public void Debug()
        {
            foreach (var (key, entry) in _entries)
            {
                var entryType = entry.GetType();

                if (!entryType.IsGenericType)
                {
                    continue;
                }

                if (entryType.GetGenericTypeDefinition() != typeof(BlackboardEntry<>))
                {
                    continue;
                }

                var valueProperty = entryType.GetProperty("Value");

                if (valueProperty == null)
                {
                    continue;
                }

                var value = valueProperty.GetValue(entry);
                UnityEngine.Debug.Log($"{key} = {value}");
            }
        }

        public BlackboardKey GetOrRegisterKey(string keyName)
        {
            if (string.IsNullOrEmpty(keyName))
            {
                throw new ArgumentException("Key name cannot be null or empty.");
            }

            if (_keyRegistry.TryGetValue(keyName, out var key))
            {
                return key;
            }

            key = new BlackboardKey(keyName);
            _keyRegistry[keyName] = key;

            return key;
        }

        public void SetValue<T>(string keyName, T value)
        {
            var key = GetOrRegisterKey(keyName);
            SetValue(key, value);
        }

        public void SetValue<T>(BlackboardKey key, T value)
        {
            if (_entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> typedEntry)
            {
                typedEntry.Value = value;
                return;
            }

            _entries[key] = new BlackboardEntry<T>(key, value);
        }

        public bool TryGetValue<T>(string keyName, out T value)
        {
            var key = GetOrRegisterKey(keyName);
            return TryGetValue(key, out value);
        }

        public bool TryGetValue<T>(BlackboardKey key, out T value)
        {
            if (_entries.TryGetValue(key, out var entry) && entry is BlackboardEntry<T> typedEntry)
            {
                value = typedEntry.Value;
                return true;
            }

            value = default;
            return false;
        }

        public bool ContainsKey(BlackboardKey key)
        {
            return _entries.ContainsKey(key);
        }

        public void Remove(BlackboardKey key)
        {
            _entries.Remove(key);
        }
        
        /// <summary>
        /// Copies all entries from the current blackboard to a new blackboard instance. Note: Keys registered in _keyRegistry but not used will not be copied.
        /// </summary>
        /// <returns>The cloned Blackboard instance</returns>
        public Blackboard Clone()
        {
            var clonedBlackboard = new Blackboard();
            
            foreach (var (key, entry) in _entries)
            {
                var entryType = entry.GetType();
                
                if (!entryType.IsGenericType || entryType.GetGenericTypeDefinition() != typeof(BlackboardEntry<>))
                {
                    continue;
                }
                
                var valueProperty = entryType.GetProperty("Value");
                
                if (valueProperty == null)
                {
                    continue;
                }
                
                var value = valueProperty.GetValue(entry);
                var valueType = valueProperty.PropertyType;
                
                var blackboardEntryType = typeof(BlackboardEntry<>).MakeGenericType(valueType);
                var clonedEntry = Activator.CreateInstance(blackboardEntryType, key, value);
                
                clonedBlackboard._entries[key] = clonedEntry;
                // Only used keys will be registered in the cloned blackboard
                clonedBlackboard._keyRegistry[key.ToString()] = key;
            }
            
            return clonedBlackboard;
        }
    }
}