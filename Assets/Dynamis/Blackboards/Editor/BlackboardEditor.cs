using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Dynamis.Blackboards;

namespace Dynamis.Blackboards.Editor
{
    [CustomEditor(typeof(Blackboard))]
    public class BlackboardEditor : UnityEditor.Editor
    {
        private Blackboard _blackboard;
        private Vector2 _scrollPosition;
        
        // 添加新条目的字段
        private string _newKey = "";
        private int _selectedTypeIndex = 0;
        private string _newStringValue = "";
        private int _newIntValue = 0;
        private float _newFloatValue = 0f;
        private bool _newBoolValue = false;
        private Vector3 _newVector3Value = Vector3.zero;
        private Color _newColorValue = Color.white;
        private UnityEngine.Object _newObjectValue;
        
        // 支持的类型列表
        private readonly string[] _supportedTypes = new string[]
        {
            "System.String",
            "System.Int32", 
            "System.Single",
            "System.Boolean",
            "UnityEngine.Vector3",
            "UnityEngine.Color",
            "UnityEngine.GameObject",
            "UnityEngine.Transform",
            "UnityEngine.Texture2D",
            "UnityEngine.Material",
            "UnityEngine.AudioClip",
            "UnityEngine.Object"
        };
        
        private readonly string[] _typeDisplayNames = new string[]
        {
            "String",
            "Int",
            "Float", 
            "Bool",
            "Vector3",
            "Color",
            "GameObject",
            "Transform",
            "Texture2D",
            "Material",
            "AudioClip",
            "Object (Generic)"
        };

        private void OnEnable()
        {
            _blackboard = (Blackboard)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Blackboard Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            // 序列化设置
            EditorGUILayout.PropertyField(serializedObject.FindProperty("_serializeWhenEditorPlaying"), 
                new GUIContent("Serialize When Editor Playing", "是否在编辑器播放模式下序列化数值"));
            
            EditorGUILayout.Space();
            
            // 添加新条目区域
            DrawAddNewEntrySection();
            
            EditorGUILayout.Space();
            
            // 现有条目列表
            DrawExistingEntriesSection();

            // 工具按钮
            EditorGUILayout.Space();
            DrawUtilityButtons();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_blackboard);
                serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawAddNewEntrySection()
        {
            EditorGUILayout.LabelField("Add New Entry", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _newKey = EditorGUILayout.TextField("Key", _newKey);
                _selectedTypeIndex = EditorGUILayout.Popup("Type", _selectedTypeIndex, _typeDisplayNames);
                
                // 根据选择的类型显示相应的输入字段
                DrawValueInputField();
                
                EditorGUILayout.Space();
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Add Entry", GUILayout.Height(25)))
                    {
                        AddNewEntry();
                    }
                    
                    if (GUILayout.Button("Clear", GUILayout.Height(25), GUILayout.Width(60)))
                    {
                        ClearNewEntryFields();
                    }
                }
            }
        }

        private void DrawValueInputField()
        {
            switch (_selectedTypeIndex)
            {
                case 0: // String
                    _newStringValue = EditorGUILayout.TextField("Value", _newStringValue);
                    break;
                case 1: // Int
                    _newIntValue = EditorGUILayout.IntField("Value", _newIntValue);
                    break;
                case 2: // Float
                    _newFloatValue = EditorGUILayout.FloatField("Value", _newFloatValue);
                    break;
                case 3: // Bool
                    _newBoolValue = EditorGUILayout.Toggle("Value", _newBoolValue);
                    break;
                case 4: // Vector3
                    _newVector3Value = EditorGUILayout.Vector3Field("Value", _newVector3Value);
                    break;
                case 5: // Color
                    _newColorValue = EditorGUILayout.ColorField("Value", _newColorValue);
                    break;
                case 6: // GameObject
                case 7: // Transform
                case 8: // Texture2D
                case 9: // Material
                case 10: // AudioClip
                case 11: // Object (Generic)
                    _newObjectValue = EditorGUILayout.ObjectField("Value", _newObjectValue, typeof(UnityEngine.Object), true);
                    break;
            }
        }

        private void DrawExistingEntriesSection()
        {
            EditorGUILayout.LabelField($"Existing Entries ({_blackboard.Entry.Count})", EditorStyles.boldLabel);
            
            if (_blackboard.Entry.Count == 0)
            {
                EditorGUILayout.HelpBox("No entries found. Add some entries above.", MessageType.Info);
                return;
            }

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(300));
            
            var entriesToRemove = new List<string>();
            
            foreach (var kvp in _blackboard.Entry)
            {
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.LabelField("Key:", GUILayout.Width(30));
                        EditorGUILayout.LabelField(kvp.Key, EditorStyles.boldLabel);
                        
                        GUILayout.FlexibleSpace();
                        
                        if (GUILayout.Button("×", GUILayout.Width(20), GUILayout.Height(16)))
                        {
                            if (EditorUtility.DisplayDialog("Delete Entry", 
                                $"Are you sure you want to delete entry '{kvp.Key}'?", "Delete", "Cancel"))
                            {
                                entriesToRemove.Add(kvp.Key);
                            }
                        }
                    }
                    
                    EditorGUILayout.LabelField("Type:", kvp.Value.Type);
                    
                    // 显示当前值
                    EditorGUILayout.LabelField("Serialized Value:", kvp.Value.Value);
                    
                    // 如果在运行时，显示运行时值的编辑界面
                    if (Application.isPlaying)
                    {
                        DrawRuntimeValueEditor(kvp.Key, kvp.Value);
                    }
                }
                
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.EndScrollView();
            
            // 删除标记的条目
            foreach (var key in entriesToRemove)
            {
                _blackboard.Entry.Remove(key);
            }
        }

        private void DrawRuntimeValueEditor(string key, SerializedBlackboardEntry entry)
        {
            EditorGUILayout.LabelField("Runtime Value Editor:", EditorStyles.miniBoldLabel);
            
            var typeName = entry.Type;
            
            try
            {
                switch (typeName)
                {
                    case "System.String":
                        if (entry.TryGetValueRuntime<string>(out var stringVal))
                        {
                            var newStringVal = EditorGUILayout.TextField("Runtime Value", stringVal);
                            if (newStringVal != stringVal)
                            {
                                _blackboard.SetValue(key, newStringVal);
                            }
                        }
                        break;
                        
                    case "System.Int32":
                        if (entry.TryGetValueRuntime<int>(out var intVal))
                        {
                            var newIntVal = EditorGUILayout.IntField("Runtime Value", intVal);
                            if (newIntVal != intVal)
                            {
                                _blackboard.SetValue(key, newIntVal);
                            }
                        }
                        break;
                        
                    case "System.Single":
                        if (entry.TryGetValueRuntime<float>(out var floatVal))
                        {
                            var newFloatVal = EditorGUILayout.FloatField("Runtime Value", floatVal);
                            if (Math.Abs(newFloatVal - floatVal) > 0.001f)
                            {
                                _blackboard.SetValue(key, newFloatVal);
                            }
                        }
                        break;
                        
                    case "System.Boolean":
                        if (entry.TryGetValueRuntime<bool>(out var boolVal))
                        {
                            var newBoolVal = EditorGUILayout.Toggle("Runtime Value", boolVal);
                            if (newBoolVal != boolVal)
                            {
                                _blackboard.SetValue(key, newBoolVal);
                            }
                        }
                        break;
                        
                    case "UnityEngine.Vector3":
                        if (entry.TryGetValueRuntime<Vector3>(out var vector3Val))
                        {
                            var newVector3Val = EditorGUILayout.Vector3Field("Runtime Value", vector3Val);
                            if (newVector3Val != vector3Val)
                            {
                                _blackboard.SetValue(key, newVector3Val);
                            }
                        }
                        break;
                        
                    case "UnityEngine.Color":
                        if (entry.TryGetValueRuntime<Color>(out var colorVal))
                        {
                            var newColorVal = EditorGUILayout.ColorField("Runtime Value", colorVal);
                            if (newColorVal != colorVal)
                            {
                                _blackboard.SetValue(key, newColorVal);
                            }
                        }
                        break;
                        
                    case "UnityEngine.GameObject":
                    case "UnityEngine.Transform":
                    case "UnityEngine.Texture2D":
                    case "UnityEngine.Material":
                    case "UnityEngine.AudioClip":
                    case "UnityEngine.Object":
                        if (entry.TryGetValueRuntime<UnityEngine.Object>(out var objectVal))
                        {
                            var newObjectVal = EditorGUILayout.ObjectField("Runtime Value", objectVal, typeof(UnityEngine.Object), true);
                            if (newObjectVal != objectVal)
                            {
                                _blackboard.SetValue(key, newObjectVal);
                            }
                        }
                        break;
                        
                    default:
                        EditorGUILayout.HelpBox($"Runtime editing not supported for type: {typeName}", MessageType.Info);
                        break;
                }
            }
            catch (Exception ex)
            {
                EditorGUILayout.HelpBox($"Error accessing runtime value: {ex.Message}", MessageType.Warning);
            }
        }

        private void DrawUtilityButtons()
        {
            EditorGUILayout.LabelField("Utilities", EditorStyles.boldLabel);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Clear All Entries"))
                {
                    if (EditorUtility.DisplayDialog("Clear All Entries", 
                        "Are you sure you want to clear all entries? This action cannot be undone.", 
                        "Clear All", "Cancel"))
                    {
                        _blackboard.Entry.Clear();
                    }
                }
                
                if (GUILayout.Button("Bake Values to Runtime") && Application.isPlaying)
                {
                    _blackboard.BakeValuesToRuntime();
                    EditorUtility.DisplayDialog("Bake Complete", "All serialized values have been baked to runtime values.", "OK");
                }
            }
        }

        private void AddNewEntry()
        {
            if (string.IsNullOrEmpty(_newKey))
            {
                EditorUtility.DisplayDialog("Invalid Key", "Key cannot be empty.", "OK");
                return;
            }
            
            if (_blackboard.Entry.ContainsKey(_newKey))
            {
                EditorUtility.DisplayDialog("Duplicate Key", $"Key '{_newKey}' already exists.", "OK");
                return;
            }
            
            var typeName = _supportedTypes[_selectedTypeIndex];
            var entry = new SerializedBlackboardEntry(_newKey, typeName, "");
            
            // 根据选择的类型设置初始值
            switch (_selectedTypeIndex)
            {
                case 0: // String
                    entry.SetValue(_newStringValue);
                    break;
                case 1: // Int
                    entry.SetValue(_newIntValue);
                    break;
                case 2: // Float
                    entry.SetValue(_newFloatValue);
                    break;
                case 3: // Bool
                    entry.SetValue(_newBoolValue);
                    break;
                case 4: // Vector3
                    entry.SetValue(_newVector3Value);
                    break;
                case 5: // Color
                    entry.SetValue(_newColorValue);
                    break;
                case 6: // GameObject
                case 7: // Transform
                case 8: // Texture2D
                case 9: // Material
                case 10: // AudioClip
                case 11: // Object (Generic)
                    entry.SetValue(_newObjectValue);
                    break;
            }
            
            _blackboard.Entry.Add(_newKey, entry);
            ClearNewEntryFields();
        }

        private void ClearNewEntryFields()
        {
            _newKey = "";
            _selectedTypeIndex = 0;
            _newStringValue = "";
            _newIntValue = 0;
            _newFloatValue = 0f;
            _newBoolValue = false;
            _newVector3Value = Vector3.zero;
            _newColorValue = Color.white;
            _newObjectValue = null;
        }
    }
}
