using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CMUDictLoader : MonoBehaviour
{
    public static Dictionary<string, string[]> Pronunciations = new Dictionary<string, string[]>();

    void Awake()
    {
        LoadCMUDict();
    }

    void LoadCMUDict()
    {
        TextAsset dictFile = Resources.Load<TextAsset>("mhyph");
        if (dictFile == null)
        {
            Debug.LogError("Failed to load cmudict.txt from Resources!");
            return;
        }

        string[] lines = dictFile.text.Split('\n');
        Debug.Log("Lines found = " + lines.Count());

        foreach (string line in lines)
        {
            string spelledWord = Regex.Replace(line, "=", "");
            string[] splitArray = line.Split('=');
            if(!Pronunciations.ContainsKey(spelledWord)) Pronunciations.Add(spelledWord, splitArray);
        }

        Debug.Log($"✅ Loaded {Pronunciations.Count} words from CMU dictionary.");
    }
}