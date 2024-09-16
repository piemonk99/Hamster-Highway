using System;
using System.IO;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class ScoreTracker
{
    public static ScoreTracker Instance;

    [DoNotSerialize] public int score;
    [SerializeField] public int bestScore;
    [DoNotSerialize] public int prevBestScore;
    [SerializeField] public int coins;

    public static void Read()
    {
        ScoreTracker readInstance;

        if (File.Exists(Path()))
            readInstance = JsonUtility.FromJson<ScoreTracker>(File.ReadAllText(Path()));
        else
            readInstance = new ScoreTracker();

        if (Instance == null)
        {
            Instance = readInstance;
            Instance.prevBestScore = Instance.bestScore;
        }
    }

    public void Write()
    {
        File.WriteAllText(Path(), JsonUtility.ToJson(this));
    }

    public static string Path()
    {
        return $"{Application.persistentDataPath}/score_data.json";
    }
}