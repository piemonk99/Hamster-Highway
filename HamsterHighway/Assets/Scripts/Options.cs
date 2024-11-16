using System;
using System.IO;
using UnityEngine;

[Serializable]
public class Options
{
    public static Options Instance;

    [SerializeField] public float Volume = 1;

    // New toggles
    [SerializeField] public bool useAccelerometer = true;
    [SerializeField] public bool usePhoneFlipping = false;
    [SerializeField] public bool cameraFollowBall = false;
    [SerializeField] public bool arMode = true;

    public static void Read()
    {
        if (File.Exists(Path()))
            Instance ??= JsonUtility.FromJson<Options>(File.ReadAllText(Path()));
        else
            Instance ??= new Options();
    }

    public void Write()
    {
        File.WriteAllText(Path(), JsonUtility.ToJson(this));
    }

    public static string Path()
    {
        return $"{Application.persistentDataPath}/options.json";
    }
}
