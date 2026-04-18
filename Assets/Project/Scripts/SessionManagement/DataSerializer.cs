using System;
using System.IO;
using Newtonsoft.Json;
using Project.Scripts.SessionManagement.Data;
using UnityEngine;

namespace Project.Scripts.SessionManagement
{
    public static class DataSerializer
    {
        private const string k_gameDataDirectoryName = "GameData";
        private const string k_gameDataFileName = "GameData.json";
        private const string k_postGameDataDirectoryName = "PostGameData";
        private const string k_postGameDataFileName = "PostGameData.json";

        public static string GameDataFilePath => Path.Combine(Application.persistentDataPath, k_gameDataDirectoryName, k_gameDataFileName);
        public static string PostGameDataFilePath => Path.Combine(Application.persistentDataPath, k_postGameDataDirectoryName, k_postGameDataFileName);

        public static void Save<T>(string filePath, T data)
        {
            try
            {
                string directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string json = JsonConvert.SerializeObject(data, Formatting.Indented);
                File.WriteAllText(filePath, json);
                Debug.Log($"[DataSerializer] Saved data to [{filePath}]");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to save data to [{filePath}].\n{exception}");
            }
        }

        public static bool TryLoad<T>(string filePath, out T data)
        {
            data = default;

            if (!File.Exists(filePath))
            {
                return false;
            }

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                T deserializedData = JsonConvert.DeserializeObject<T>(json);
                if (ReferenceEquals(deserializedData, null) && !typeof(T).IsValueType)
                {
                    return false;
                }

                data = deserializedData;
                Debug.Log($"[DataSerializer] Loaded data from [{filePath}]");
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to load data from [{filePath}].\n{exception}");
                return false;
            }
        }

        public static void Delete(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                File.Delete(filePath);
                Debug.Log($"[DataSerializer] Deleted data file [{filePath}]");
            }
            catch (Exception exception)
            {
                Debug.LogError($"Failed to delete data file [{filePath}].\n{exception}");
            }
        }

        public static void SavePostGameData(PostGameData postGameData)
        {
            Save(PostGameDataFilePath, postGameData);
        }

        public static void SaveGameData(GameData gameData)
        {
            Save(GameDataFilePath, gameData);
        }

        public static bool TryLoadGameData(out GameData gameData)
        {
            return TryLoad(GameDataFilePath, out gameData);
        }

        public static bool TryLoadPostGameData(out PostGameData postGameData)
        {
            return TryLoad(PostGameDataFilePath, out postGameData);
        }

        public static void DeletePostGameData()
        {
            Delete(PostGameDataFilePath);
        }
    }
}
