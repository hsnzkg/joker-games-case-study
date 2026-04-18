using System.IO;
using Project.Scripts.SessionManagement;
using UnityEngine;

namespace Project.Scripts.BetManagement.Bet
{
    public static class BetBoardPersistence
    {
        private const string k_betBoardDirectoryName = "BetBoardData";
        private const string k_betBoardFileName = "BetBoardData.json";
        public static string BetBoardDataFilePath => Path.Combine(Application.persistentDataPath, k_betBoardDirectoryName, k_betBoardFileName);

        public static void Save(BoardData data)
        {
            DataSerializer.Save(BetBoardDataFilePath, data);
        }

        public static bool TryLoad(out BoardData data)
        {
            return DataSerializer.TryLoad(BetBoardDataFilePath, out data);
        }

        public static void Delete()
        {
            DataSerializer.Delete(BetBoardDataFilePath);
        }
    }
}