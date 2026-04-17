using Project.Scripts.Roulette.RouletteDesk;
using Project.Scripts.Roulette.Utility;
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
        }
    }
#endif
}