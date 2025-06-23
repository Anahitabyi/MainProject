using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string savePath => Application.persistentDataPath + "/savefile.json";

    public static void SaveGame(GameData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);
    }

    public static GameData LoadGame()
    {
        if (!File.Exists(savePath))
            return null;

        string json = File.ReadAllText(savePath);
        return JsonUtility.FromJson<GameData>(json);
    }
    public static void DeleteSave()
        {
            if (File.Exists(savePath))
                File.Delete(savePath);
        }

    public static bool SaveExists() => File.Exists(savePath);
}