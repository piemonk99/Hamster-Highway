using System;
using System.IO;
using UnityEngine;

[Serializable]
public class ScoreTracker
{
    private static ScoreTracker instance;

    public static ScoreTracker Instance
    {
        get
        {
            // Initialize the instance if it is null
            if (instance == null)
            {
                Read();  // This ensures Instance is initialized even if accessed before calling Read
            }
            return instance;
        }
    }

    [NonSerialized] public int score;
    [SerializeField] public int bestScore;
    [NonSerialized] public int prevBestScore;
    [SerializeField] public int coins;

    private ScoreTracker() { }  // Private constructor to prevent direct instantiation

    public static void Read()
    {
        if (instance != null) return;  // Only read if Instance is not already set

        ScoreTracker readInstance;

        if (File.Exists(Path()))
            readInstance = JsonUtility.FromJson<ScoreTracker>(File.ReadAllText(Path()));
        else
            readInstance = new ScoreTracker();

        instance = readInstance;
        instance.prevBestScore = instance.bestScore;
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
