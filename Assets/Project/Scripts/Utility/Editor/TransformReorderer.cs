using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Utility.Editor
{
    public class TransformReorderer : EditorWindow
    {
        private Transform m_parent;

        [MenuItem("Tools/Reorder Children By Name Number")]
        private static void ShowWindow()
        {
            GetWindow<TransformReorderer>("Child Reorder"); 
        }

        private void OnGUI()
        {
            GUILayout.Label("Transform Reorderer Tool", EditorStyles.boldLabel);

            m_parent = (Transform)EditorGUILayout.ObjectField(
                "Parent Transform",
                m_parent,
                typeof(Transform),
                true);

            EditorGUILayout.Space();

            if (GUILayout.Button("Use Selected Transform"))
            {
                if (Selection.activeTransform != null)
                {
                    m_parent = Selection.activeTransform;
                }
            }

            UnityEngine.GUI.enabled = m_parent != null;

            if (GUILayout.Button("Reorder Children"))
            {
                ReorderChildren(m_parent);
            }

            UnityEngine.GUI.enabled = true;
        }

        private static void ReorderChildren(Transform parent)
        {
            if (parent == null)
            {
                Debug.LogError("Parent transform is null.");
                return;
            }

            List<Transform> children = new List<Transform>();

            for (int i = 0; i < parent.childCount; i++)
            {
                children.Add(parent.GetChild(i));
            }

            children.Sort((a, b) =>
            {
                int aNumber = ExtractTrailingNumber(a.name);
                int bNumber = ExtractTrailingNumber(b.name);

                int compare = aNumber.CompareTo(bNumber);
                if (compare != 0)
                    return compare;

                return string.Compare(a.name, b.name, StringComparison.Ordinal);
            });

            Undo.RecordObject(parent, "Reorder Children By Name Number");

            for (int i = 0; i < children.Count; i++)
            {
                children[i].SetSiblingIndex(i);
            }

            EditorUtility.SetDirty(parent);

            Debug.Log($"Reordered {children.Count} children under '{parent.name}'.");
        }

        private static int ExtractTrailingNumber(string objectName)
        {
            if (string.IsNullOrWhiteSpace(objectName))
                return int.MaxValue;

            Match match = Regex.Match(objectName, @"(\d+)$");
            if (match.Success && int.TryParse(match.Value, out int result))
                return result;

            return int.MaxValue;
        }
    }
}