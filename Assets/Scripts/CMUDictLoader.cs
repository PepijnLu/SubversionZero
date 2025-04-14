using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class CMUDictLoader : MonoBehaviour
{
    public Dictionary<string, string[]> pronunciations;
    List<string> firstFiftyWords = new();
    void Awake()
    {
        pronunciations = new Dictionary<string, string[]>();
        LoadCMUDict();
    }

    void LoadCMUDict()
    {
        int i = 0;
        TextAsset dictFile = Resources.Load<TextAsset>("mhyph");
        if (dictFile == null)
        {
            Debug.LogError("Failed to load cmudict.txt from Resources!");
            return;
        }

        string[] lines = dictFile.text.Split("\r\n");
        Debug.Log("Lines found = " + lines.Count());

        foreach (string line in lines)
        {
            string spelledWord = Regex.Replace(line, "=", "");
            string[] splitArray = line.Split('=');
            if(!pronunciations.ContainsKey(spelledWord)) 
            {
                if(i < 50)
                {
                    Debug.Log($"Adding to dict: {spelledWord}, {splitArray}");
                    firstFiftyWords.Add(spelledWord);
                    i++;
                }
                pronunciations.Add(spelledWord, splitArray);
            }
        }

        Debug.Log($"✅ Loaded {pronunciations.Count} words from CMU dictionary.");

        i = 0;
        for(int i2 = 0; i2 < firstFiftyWords.Count; i2++)
        {
            Debug.Log($"Reading from dict: {firstFiftyWords[i]} : {pronunciations[firstFiftyWords[i]]}");
        }
    }
}