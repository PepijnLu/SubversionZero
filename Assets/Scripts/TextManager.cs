using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using TMPro;
using UnityEngine;
using FMODUnity;

public class TextManager : MonoBehaviour
{
    public string testPhrase;
    [SerializeField] public float timeBetweenSyllables, timeBetweenWords, timeBetweenSentences, fadeInTime;
    [SerializeField] TextMeshProUGUI alphaText, fullText;
    [SerializeField] Transform textHolder;
    [SerializeField] Image nameTag;
    List<string> sentenceEndingPunctiation, sentenceContinueingPunctuation;
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] CMUDictLoader cmuDictLoader;
    [Header("Ink JSON")]
    [SerializeField] TextAsset inkJSON;
    public bool showingText;

    bool IsVowel(char c)
    {
        return "aeiouyhAEIOUYH".IndexOf(c) >= 0;
    }

    void Start()
    {
        sentenceEndingPunctiation = new()
        {
            ".",
            "?",
            "!",
            "..."
        };

        sentenceContinueingPunctuation = new()
        {
            ",",
            ":",
            ";"
        };

        //dialogueManager.EnterDialogueMode(inkJSON);
        //StartCoroutine(DisplayPhraseInSyllables(testPhrase, "default", timeBetweenSyllables, timeBetweenWords, timeBetweenSentences));
    }

    void Update() { }

    public IEnumerator DisplayPhraseInSyllables(string _phrase, string tag, float _timeBetweenSyllables, float _timeBetweenWords, float _timeBetweenSentences)
    {
        showingText = true;
        Debug.Log("Text tag = " + tag);
        fullText.text = "";
        alphaText.text = "";

        List<string> words = GetWordsInPhrase(_phrase);
        bool capitalizeNextWord = true;
        bool isItalic = _phrase.Contains("<i>") || _phrase.Contains("</i>");

        for (int i = 0; i < words.Count; i++)
        {
            string _word = words[i];
            if (string.IsNullOrWhiteSpace(_word)) continue;

            if (!sentenceEndingPunctiation.Contains(_word) && !sentenceContinueingPunctuation.Contains(_word))
            {
                List<string> syllables = SplitSyllables(_word, capitalizeNextWord).ToList();
                capitalizeNextWord = false;

                for (int i2 = 0; i2 < syllables.Count; i2++)
                {
                    string syll = syllables[i2];

                    bool nextIsPunct = (i + 1 < words.Count) && sentenceEndingPunctiation.Contains(words[i + 1]);
                    string punct = nextIsPunct ? words[i + 1] : null;
                    bool startsWithVowel = syll.Length > 0 && IsVowel(syll[0]);

                    int syllableType;
                    if (!nextIsPunct)
                    {
                        syllableType = startsWithVowel ? 1 : 0;
                    }
                    else
                    {
                        switch (punct)
                        {
                            case ".": syllableType = startsWithVowel ? 5 : 2; break;
                            case "?": syllableType = startsWithVowel ? 6 : 3; break;
                            case "!": syllableType = startsWithVowel ? 7 : 4; break;
                            default: syllableType = startsWithVowel ? 1 : 0; break;
                        }
                    }

                    Debug.Log($"Playing syllable sound -> nextIsPunct: {nextIsPunct}, syllableType: {syllableType}, syllable: '{syll}'");
                    
                    if(!isItalic) FModManager.instance.PlaySyllableSound(syllableType);

                    alphaText.text += syll;
                    StartCoroutine(FadeInText(fadeInTime));
                    yield return new WaitForSeconds(_timeBetweenSyllables);
                }

                if (i + 1 < words.Count && !sentenceEndingPunctiation.Contains(words[i + 1]))
                {
                    if(!sentenceContinueingPunctuation.Contains(words[i + 1])) alphaText.text += " ";
                    yield return new WaitForSeconds(_timeBetweenWords);
                }
            }
            else if (sentenceEndingPunctiation.Contains(_word))
            {
                alphaText.text += _word + " ";
                StartCoroutine(FadeInText(fadeInTime));
                capitalizeNextWord = true;
                yield return new WaitForSeconds(_timeBetweenSentences);
            }
            else if (sentenceContinueingPunctuation.Contains(_word))
            {
                alphaText.text += _word + " ";
                StartCoroutine(FadeInText(fadeInTime));
                capitalizeNextWord = false;
                yield return new WaitForSeconds(_timeBetweenWords);
            }
        }

        showingText = false;
    }

    IEnumerator FadeInText(float duration)
    {
        TextMeshProUGUI newAlphaInstance = Instantiate(alphaText, textHolder);
        float startAlpha = newAlphaInstance.color.a;
        float elapsedTime = 0f;
        float targetAlpha = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            SetAlpha(alpha, newAlphaInstance);
            yield return null;
        }

        SetAlpha(targetAlpha, newAlphaInstance);
        SwitchTextToFull(newAlphaInstance);
    }

    void SwitchTextToFull(TextMeshProUGUI _alphaText)
    {
        fullText.text = _alphaText.text;
        Color color = _alphaText.color;
        color.a = 0;
        _alphaText.color = color;
        Destroy(_alphaText.gameObject);
    }

    private void SetAlpha(float alpha, TextMeshProUGUI _alphaText)
    {
        Color color = _alphaText.color;
        color.a = alpha;
        _alphaText.color = color;
    }

    List<string> GetWordsInPhrase(string _phrase)
    {
        return _phrase.Split().ToList();
    }

    public void DisableText()
    {
        fullText.text = "";
        alphaText.text = "";
    }

    public string[] SplitSyllables(string word, bool firstWord)
    {
        string[] completeWordInArray = { word };
        var dictToCheck = cmuDictLoader.pronunciations;
        Debug.Log($"Split Syllables Input: {word}, {firstWord}");

        //Decapitalize the word if necessary to check
        if ((firstWord || word == "i") && (word != ""))
        {
            word = word.ToLower();
        }

        string[] foundArray;

        //Account for plural words
        if (!dictToCheck.ContainsKey(word) && word.Length > 0)
        {
            //Remove the last character
            string removedChar = word[word.Length - 1].ToString();
            Debug.Log($"123: Removed {removedChar} from {word}");
            word = word.Substring(0, word.Length - 1);

            //Return empty if wordt still hasn't been found
            if (!dictToCheck.ContainsKey(word) || (removedChar != "s" || removedChar != "d")) 
            {
                Debug.LogWarning($"NotFound: {word}");
                return completeWordInArray;
            }

            //Otherwise find the string array
            foundArray = dictToCheck[word];

            //And add the last letter back in
            Debug.Log($"123: Adding {removedChar} back to {foundArray[foundArray.Length - 1]}");
            
            foundArray = (string[])dictToCheck[word].Clone();
            foundArray[foundArray.Length - 1] += removedChar;

            //Recapitalize the first word
            if ((firstWord || word == "i") && (word != ""))
            {
                Debug.Log($"123: Capitalizing first letter, firstword: {firstWord}, word = i: {word == "i" }");
                foundArray = (string[])dictToCheck[word].Clone();
                foundArray[0] = char.ToUpper(foundArray[0][0]) + foundArray[0].Substring(1);
            }
            return foundArray;
        }

        //Return empty if wordt still hasn't been found
        if (!dictToCheck.ContainsKey(word)) 
        {
            Debug.LogWarning($"NotFound: {word}");
            return completeWordInArray;
        }

        //Otherwise get the string array from the dictionary
        foundArray = dictToCheck[word];

        //Recapitalize the first word
        if ((firstWord || word == "i") && (word != ""))
        {
            Debug.Log($"123: Capitalizing first letter, firstword: {firstWord}, word = i: {word == "i" }");
            foundArray = (string[])dictToCheck[word].Clone();
            foundArray[0] = char.ToUpper(foundArray[0][0]) + foundArray[0].Substring(1);
        }

        Debug.Log($"Split Syllables Output: {foundArray.Length}");
        return foundArray;
    }
}
