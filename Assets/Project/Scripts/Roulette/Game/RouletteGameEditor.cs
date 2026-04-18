using System.IO;
using Project.Scripts.Roulette.Desk;
using Project.Scripts.Roulette.Utility;
using Project.Scripts.SessionManagement;
using Unity.CodeEditor;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Roulette.Game
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RouletteGame))]
    public class RouletteWheelControllerEditor : Editor
    {
        private SaveFileSelection m_selectedSaveFile = SaveFileSelection.PostGameData;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            RouletteGame rouletteGame = (RouletteGame)target;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Post Game Save Path:\n{DataSerializer.PostGameDataFilePath}", MessageType.Info);
            EditorGUILayout.HelpBox($"Game Data Save Path:\n{DataSerializer.GameDataFilePath}", MessageType.Info);
            m_selectedSaveFile = (SaveFileSelection)EditorGUILayout.EnumPopup("Selected Save File", m_selectedSaveFile);

            if (GUILayout.Button("Open Selected Save File"))
            {
                OpenSelectedSaveFile(m_selectedSaveFile);
            }

            if (GUILayout.Button("Start Random Game"))
            {
                rouletteGame.StartGame();
            }
            
            if (GUILayout.Button("Start Deterministic Game To Number 13"))
            {
                rouletteGame.StartDeterministicGame(13.GetSlotInfoBySlotNumber().Index);
            }
            
            if (GUILayout.Button("Start Deterministic Game To Black"))
            {
                rouletteGame.StartDeterministicGame(SlotColor.BLACK.GetRandomSlotInfoByColor().Index);
            }
            
            if (GUILayout.Button("Start Deterministic Game To Red"))
            {
                rouletteGame.StartDeterministicGame(SlotColor.RED.GetRandomSlotInfoByColor().Index);
            }
            
            if (GUILayout.Button("Start Deterministic Game To Green"))
            {
                rouletteGame.StartDeterministicGame(SlotColor.GREEN.GetRandomSlotInfoByColor().Index);
            }

            EditorGUILayout.Space();

            if (GUILayout.Button("Delete Post Game Save"))
            {
                DataSerializer.DeletePostGameData();
            }
        }

        private static void OpenSelectedSaveFile(SaveFileSelection selectedSaveFile)
        {
            string filePath = selectedSaveFile switch
            {
                SaveFileSelection.PostGameData => DataSerializer.PostGameDataFilePath,
                SaveFileSelection.GameData => DataSerializer.GameDataFilePath,
                _ => DataSerializer.PostGameDataFilePath
            };

            string directoryPath = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            if (!File.Exists(filePath))
            {
                File.WriteAllText(filePath, "{}");
            }

            CodeEditor.CurrentEditor.OpenProject(filePath, 0, 0);
        }

        private enum SaveFileSelection
        {
            PostGameData,
            GameData
        }
    }
#endif
}
