using System;
using System.Collections.Generic;
using UnityEngine;

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
    public class SerializableBlackboardEntry
    {
        public string keyName;
        public string typeName;
        public string serializedValue;

        public SerializableBlackboardEntry() { }

        public SerializableBlackboardEntry(string keyName, string typeName, string serializedValue)
        {
            this.keyName = keyName;
            this.typeName = typeName;
            this.serializedValue = serializedValue;
        }
    }

    [CreateAssetMenu(fileName = "New Blackboard", menuName = "Dynamis/Blackboard")]
    public class Blackboard : ScriptableObject, IBlackboard, ISerializationCallbackReceiver
    {
        [SerializeField] private List<SerializableBlackboardEntry> _serializableEntries = new();
        [SerializeField] private List<string> _serializableKeys = new();

        private Dictionary<string, BlackboardKey> _keyRegistry;
        private Dictionary<BlackboardKey, object> _entries;

        private void OnEnable()
        {
            _keyRegistry ??= new Dictionary<string, BlackboardKey>();
            _entries ??= new Dictionary<BlackboardKey, object>();
        }

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
            var clonedBlackboard = CreateInstance<Blackboard>();

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

        public void OnBeforeSerialize()
        {
            _serializableEntries.Clear();
            _serializableKeys.Clear();

            if (_keyRegistry != null)
            {
                foreach (var kvp in _keyRegistry)
                {
                    _serializableKeys.Add(kvp.Key);
                }
            }

            if (_entries != null)
            {
                foreach (var kvp in _entries)
                {
                    var key = kvp.Key;
                    var entry = kvp.Value;
                    var keyName = key.ToString();

                    var entryType = entry.GetType();
                    if (!entryType.IsGenericType || entryType.GetGenericTypeDefinition() != typeof(BlackboardEntry<>))
                        continue;

                    var valueProperty = entryType.GetProperty("Value");
                    if (valueProperty == null)
                        continue;

                    var value = valueProperty.GetValue(entry);
                    var valueType = valueProperty.PropertyType;

                    try
                    {
                        string serializedValue;
                        
                        // 处理基本类型和Unity可序列化类型
                        if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(Vector2) ||
                            valueType == typeof(Vector3) || valueType == typeof(Vector4) || valueType == typeof(Color) ||
                            valueType == typeof(Quaternion) || valueType.IsEnum)
                        {
                            // 创建一个包装器类型来序列化值
                            var wrapperType = typeof(SerializableValueWrapper<>).MakeGenericType(valueType);
                            var wrapper = Activator.CreateInstance(wrapperType);
                            wrapperType.GetField("value").SetValue(wrapper, value);
                            serializedValue = JsonUtility.ToJson(wrapper);
                        }
                        else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                        {
                            // Unity对象存储引用
                            var unityObj = value as UnityEngine.Object;
                            var wrapper = new UnityObjectWrapper 
                            { 
                                instanceID = unityObj != null ? unityObj.GetInstanceID() : 0,
                                isNull = unityObj == null
                            };
                            serializedValue = JsonUtility.ToJson(wrapper);
                        }
                        else if (valueType.IsSerializable || valueType.GetCustomAttributes(typeof(SerializableAttribute), false).Length > 0)
                        {
                            // 自定义可序列化类型
                            serializedValue = JsonUtility.ToJson(value);
                        }
                        else
                        {
                            // 不支持的类型，跳过
                            UnityEngine.Debug.LogWarning($"Type '{valueType.Name}' is not serializable and will be skipped for key '{keyName}'");
                            continue;
                        }

                        var serializableEntry = new SerializableBlackboardEntry(
                            keyName,
                            valueType.AssemblyQualifiedName,
                            serializedValue
                        );

                        _serializableEntries.Add(serializableEntry);
                    }
                    catch (Exception e)
                    {
                        UnityEngine.Debug.LogWarning($"Failed to serialize blackboard entry for key '{keyName}': {e.Message}");
                    }
                }
            }
        }

        public void OnAfterDeserialize()
        {
            _keyRegistry = new Dictionary<string, BlackboardKey>();
            _entries = new Dictionary<BlackboardKey, object>();

            // 恢复键注册表
            foreach (var keyName in _serializableKeys)
            {
                if (!string.IsNullOrEmpty(keyName))
                {
                    _keyRegistry[keyName] = new BlackboardKey(keyName);
                }
            }

            // 恢复条目
            foreach (var serializableEntry in _serializableEntries)
            {
                if (string.IsNullOrEmpty(serializableEntry.keyName) || string.IsNullOrEmpty(serializableEntry.typeName))
                    continue;

                try
                {
                    var valueType = Type.GetType(serializableEntry.typeName);
                    if (valueType == null)
                    {
                        UnityEngine.Debug.LogWarning($"Could not find type '{serializableEntry.typeName}' for blackboard key '{serializableEntry.keyName}'");
                        continue;
                    }

                    var key = GetOrRegisterKey(serializableEntry.keyName);
                    object value;

                    if (valueType.IsPrimitive || valueType == typeof(string) || valueType == typeof(Vector2) ||
                        valueType == typeof(Vector3) || valueType == typeof(Vector4) || valueType == typeof(Color) ||
                        valueType == typeof(Quaternion) || valueType.IsEnum)
                    {
                        // 反序列化基本类型
                        var wrapperType = typeof(SerializableValueWrapper<>).MakeGenericType(valueType);
                        var wrapper = JsonUtility.FromJson(serializableEntry.serializedValue, wrapperType);
                        value = wrapperType.GetField("value").GetValue(wrapper);
                    }
                    else if (typeof(UnityEngine.Object).IsAssignableFrom(valueType))
                    {
                        // Unity对象反序列化
                        var wrapper = JsonUtility.FromJson<UnityObjectWrapper>(serializableEntry.serializedValue);
                        
                        if (wrapper.isNull)
                        {
                            value = null;
                        }
                        else
                        {
#if UNITY_EDITOR
                            // 在编辑器中，我们可以通过InstanceID恢复对象引用
                            value = UnityEditor.EditorUtility.InstanceIDToObject(wrapper.instanceID);
#else
                            // 在运行时，我们无法通过InstanceID恢复对象，所以设置为null
                            // 这是Unity序列化的限制，需要在编辑器中处理对象引用
                            value = null;
#endif
                        }
                    }
                    else
                    {
                        // 自定义可序列化类型
                        value = JsonUtility.FromJson(serializableEntry.serializedValue, valueType);
                    }

                    var blackboardEntryType = typeof(BlackboardEntry<>).MakeGenericType(valueType);
                    var blackboardEntry = Activator.CreateInstance(blackboardEntryType, key, value);
                    _entries[key] = blackboardEntry;
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogWarning($"Failed to deserialize blackboard entry for key '{serializableEntry.keyName}': {e.Message}");
                }
            }
        }
    }

    [Serializable]
    internal class SerializableValueWrapper<T>
    {
        public T value;
    }

    [Serializable]
    internal class UnityObjectWrapper
    {
        public int instanceID;
        public bool isNull;
    }
}

