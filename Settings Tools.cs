using System.Collections;
using System.Collections.Generic;
using Drydock;
using UnityEngine;
using System.IO;

public class SettingsTools : MonoBehaviour
{
    static string scenarios_path = Application.streamingAssetsPath + "/Scenarios/";

    private static string CleanPath()
    {
        string output;
        if (scenarios_path.Contains(".app"))
        {
            output = Path.GetDirectoryName(Path.GetDirectoryName(Application.dataPath)) + "/Scenarios/";
        }
        else
        {
            output = scenarios_path;
        }
        return output;
    }

    public static Dock LoadSettings()
    {
        Dock output;

        StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/settings.txt");
        string jsonString = sr.ReadToEnd();
        Dock tempDock = JsonUtility.FromJson<Dock>(jsonString);
        if (tempDock.capstans == null)
        {
            tempDock.capstans = new Capstan[6];
        }
        if (tempDock.keel_blocks == null)
        {
            tempDock.keel_blocks = new KeelBlock[3];
            for (int i = 0; i < tempDock.keel_blocks.Length; i++)
            {
                tempDock.keel_blocks[i] = new KeelBlock();
            }
        }
        output = new Dock(tempDock);
        sr.Close();

        return output;
    }

    public static void SaveSettings(Dock drydock)
    {
        StreamWriter sw = new StreamWriter(Application.streamingAssetsPath + "/settings.txt");
        sw.WriteLine(JsonUtility.ToJson(drydock));
        sw.Close();
    }

    public static List<string> ListScenarios()
    {
        List<string> output = new List<string>();

        output.AddRange(Directory.GetFiles(scenarios_path));

        for (int i = 0; i < output.Count; i++)
        {
            output[i] = Path.GetFileName(output[i]);
        }

        return output;
    }

    public static Dock LoadScenario(string scenario)
    {
        Dock output;

        StreamReader sr = new StreamReader(scenarios_path + scenario);
        string jsonString = sr.ReadToEnd();
        output = JsonUtility.FromJson<Dock>(jsonString);
        sr.Close();

        return output;
    }

    public static void SaveScenario(string name, Dock drydock)
    {
        if (!name.Contains(".txt"))
        {
            name += ".txt";
        }
        StreamWriter sw = new StreamWriter(scenarios_path + name);
        sw.WriteLine(JsonUtility.ToJson(drydock));
        sw.Close();
    }

    public static bool CheckGameObject(GameObject g, string attempt)
    {
        if (g == null)
        {
            Debug.LogWarning("Finding GameObject " + attempt + " failed!");
            return false;
        }
        else
        {
            return true;
        }
    }
}
