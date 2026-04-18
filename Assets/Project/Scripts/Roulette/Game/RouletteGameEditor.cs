using Project.Scripts.Roulette.Desk;
using Project.Scripts.Roulette.Utility;
using Project.Scripts.SessionManagement;
using UnityEditor;
using UnityEngine;

namespace Project.Scripts.Roulette.Game
{
#if UNITY_EDITOR
    [CustomEditor(typeof(RouletteGame))]
    public class RouletteWheelControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            RouletteGame rouletteGame = (RouletteGame)target;

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox($"Post Game Save Path:\n{DataSerializer.PostGameDataFilePath}", MessageType.Info);

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
    }
#endif
}
