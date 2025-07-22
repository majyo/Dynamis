using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Dynamis.Blackboards;

namespace Dynamis.Blackboards.Editor
{
    [CustomEditor(typeof(Blackboard))]
    public class BlackboardEditor : UnityEditor.Editor
    {
        private string newKey = "";
        private int selectedTypeIndex = 0;
        private string newValue = "";
        private Vector2 scrollPosition;
        
        private readonly string[] supportedTypes = {
            "System.String",
            "System.Int32", 
            "System.Single",
            "System.Boolean",
            "UnityEngine.Vector2",
            "UnityEngine.Vector3",
            "UnityEngine.Color"
        };
        
        private readonly string[] typeDisplayNames = {
            "String",
            "Int",
            "Float", 
            "Bool",
            "Vector2",
            "Vector3",
            "Color"
        };
        
        public override void OnInspectorGUI()
        {
            Blackboard blackboard = (Blackboard)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Blackboard管理器", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // 添加新条目区域
            DrawAddNewEntrySection(blackboard);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("当前条目", EditorStyles.boldLabel);
            
            // 显示现有条目
            DrawExistingEntries(blackboard);
            
            // 工具按钮
            EditorGUILayout.Space();
            DrawToolButtons(blackboard);
            
            if (GUI.changed)
            {
                EditorUtility.SetDirty(blackboard);
            }
        }
        
        private void DrawAddNewEntrySection(Blackboard blackboard)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("添加新条目", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("键名:", GUILayout.Width(40));
            newKey = EditorGUILayout.TextField(newKey);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("类型:", GUILayout.Width(40));
            selectedTypeIndex = EditorGUILayout.Popup(selectedTypeIndex, typeDisplayNames);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("值:", GUILayout.Width(40));
            newValue = DrawValueField(supportedTypes[selectedTypeIndex], newValue);
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("添加条目", GUILayout.Width(80)))
            {
                AddNewEntry(blackboard);
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        
        private void DrawExistingEntries(Blackboard blackboard)
        {
            if (blackboard.Entries.Count == 0)
            {
                EditorGUILayout.HelpBox("暂无条目", MessageType.Info);
                return;
            }
            
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, "box");
            
            for (int i = blackboard.Entries.Count - 1; i >= 0; i--)
            {
                var entry = blackboard.Entries[i];
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.BeginHorizontal();
                
                // 键名（可编辑）
                EditorGUILayout.LabelField("键:", GUILayout.Width(30));
                string newKeyName = EditorGUILayout.TextField(entry.Key, GUILayout.Width(120));
                if (newKeyName != entry.Key)
                {
                    entry.Key = newKeyName;
                    blackboard.ValidateEntries();
                }
                
                GUILayout.FlexibleSpace();
                
                // 删除按钮
                if (GUILayout.Button("删除", GUILayout.Width(50)))
                {
                    blackboard.RemoveEntryAt(i);
                    continue;
                }
                
                EditorGUILayout.EndHorizontal();
                
                // 类型显示
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("类型:", GUILayout.Width(30));
                string typeName = GetDisplayTypeName(entry.Type);
                EditorGUILayout.LabelField(typeName, GUILayout.Width(120));
                EditorGUILayout.EndHorizontal();
                
                // 值编辑
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("值:", GUILayout.Width(30));
                string newValue = DrawValueField(entry.Type, entry.Value);
                if (newValue != entry.Value)
                {
                    entry.Value = newValue;
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(2);
            }
            
            EditorGUILayout.EndScrollView();
        }
        
        private void DrawToolButtons(Blackboard blackboard)
        {
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Button("清空所有条目"))
            {
                if (EditorUtility.DisplayDialog("确认", "确定要清空所有条目吗？", "确定", "取消"))
                {
                    blackboard.Clear();
                }
            }
            
            if (GUILayout.Button("验证条目"))
            {
                blackboard.ValidateEntries();
                EditorUtility.DisplayDialog("完成", "条目验证完成", "确定");
            }
            
            EditorGUILayout.EndHorizontal();
        }
        
        private string DrawValueField(string typeName, string currentValue)
        {
            try
            {
                switch (typeName)
                {
                    case "System.String":
                        return EditorGUILayout.TextField(currentValue ?? "");
                        
                    case "System.Int32":
                        int intVal = string.IsNullOrEmpty(currentValue) ? 0 : int.Parse(currentValue);
                        return EditorGUILayout.IntField(intVal).ToString();
                        
                    case "System.Single":
                        float floatVal = string.IsNullOrEmpty(currentValue) ? 0f : float.Parse(currentValue);
                        return EditorGUILayout.FloatField(floatVal).ToString();
                        
                    case "System.Boolean":
                        bool boolVal = string.IsNullOrEmpty(currentValue) ? false : bool.Parse(currentValue);
                        return EditorGUILayout.Toggle(boolVal).ToString();
                        
                    case "UnityEngine.Vector2":
                        Vector2 vec2Val = string.IsNullOrEmpty(currentValue) ? Vector2.zero : JsonUtility.FromJson<Vector2>(currentValue);
                        Vector2 newVec2 = EditorGUILayout.Vector2Field("", vec2Val);
                        return JsonUtility.ToJson(newVec2);
                        
                    case "UnityEngine.Vector3":
                        Vector3 vec3Val = string.IsNullOrEmpty(currentValue) ? Vector3.zero : JsonUtility.FromJson<Vector3>(currentValue);
                        Vector3 newVec3 = EditorGUILayout.Vector3Field("", vec3Val);
                        return JsonUtility.ToJson(newVec3);
                        
                    case "UnityEngine.Color":
                        Color colorVal = string.IsNullOrEmpty(currentValue) ? Color.white : JsonUtility.FromJson<Color>(currentValue);
                        Color newColor = EditorGUILayout.ColorField(colorVal);
                        return JsonUtility.ToJson(newColor);
                        
                    default:
                        return EditorGUILayout.TextField(currentValue ?? "");
                }
            }
            catch (Exception)
            {
                return EditorGUILayout.TextField(currentValue ?? "");
            }
        }
        
        private void AddNewEntry(Blackboard blackboard)
        {
            if (string.IsNullOrEmpty(newKey))
            {
                EditorUtility.DisplayDialog("错误", "键名不能为空", "确定");
                return;
            }
            
            if (blackboard.HasKey(newKey))
            {
                if (!EditorUtility.DisplayDialog("确认", $"键 '{newKey}' 已存在，是否覆盖？", "覆盖", "取消"))
                {
                    return;
                }
            }
            
            string typeName = supportedTypes[selectedTypeIndex];
            string serializedValue = SerializeValueForType(typeName, newValue);
            
            blackboard.AddEntry(newKey, typeName, serializedValue);
            
            // 清空输入
            newKey = "";
            newValue = "";
            selectedTypeIndex = 0;
        }
        
        private string SerializeValueForType(string typeName, string value)
        {
            try
            {
                switch (typeName)
                {
                    case "System.String":
                        return value ?? "";
                        
                    case "System.Int32":
                        return (string.IsNullOrEmpty(value) ? 0 : int.Parse(value)).ToString();
                        
                    case "System.Single":
                        return (string.IsNullOrEmpty(value) ? 0f : float.Parse(value)).ToString();
                        
                    case "System.Boolean":
                        return (string.IsNullOrEmpty(value) ? false : bool.Parse(value)).ToString();
                        
                    case "UnityEngine.Vector2":
                    case "UnityEngine.Vector3":
                    case "UnityEngine.Color":
                        return string.IsNullOrEmpty(value) ? "{}" : value;
                        
                    default:
                        return value ?? "";
                }
            }
            catch (Exception)
            {
                return "";
            }
        }
        
        private string GetDisplayTypeName(string fullTypeName)
        {
            for (int i = 0; i < supportedTypes.Length; i++)
            {
                if (supportedTypes[i] == fullTypeName)
                {
                    return typeDisplayNames[i];
                }
            }
            return "Unknown";
        }
    }
}
