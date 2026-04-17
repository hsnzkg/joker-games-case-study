using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Roulette.Utility.ColliderGeneration.Editor
{
    [CustomEditor(typeof(ColliderGenerator))]
    public class ColliderGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(10);

            ColliderGenerator spawner = (ColliderGenerator)target;

            UnityEngine.GUI.backgroundColor = Color.green;
            if (GUILayout.Button("Generate Spheres"))
            {
                spawner.Generate();
                EditorUtility.SetDirty(spawner);
            }

            UnityEngine.GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Clear Spheres"))
            {
                spawner.ClearChildren();
                EditorUtility.SetDirty(spawner);
            }

            UnityEngine.GUI.backgroundColor = Color.white;
        }
    }
}