using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CMUDictLoader : MonoBehaviour
{
    public Dictionary<string, string[]> pronunciations;
    void Awake()
    {
        pronunciations = new Dictionary<string, string[]>();
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
            if(!pronunciations.ContainsKey(spelledWord)) pronunciations.Add(spelledWord, splitArray);
        }

        Debug.Log($"✅ Loaded {pronunciations.Count} words from CMU dictionary.");
    }
}