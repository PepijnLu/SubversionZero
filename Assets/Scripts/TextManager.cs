using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

public class TextManager : MonoBehaviour
{
    public string testPhrase;
    [SerializeField] public float timeBetweenSyllables, timeBetweenWords, timeBetweenSentences, fadeInTime;
    [SerializeField] TextMeshProUGUI alphaText, fullText;
    [SerializeField] Transform textHolder;
    List<string> sentenceEndingPunctiation;
    [SerializeField] DialogueManager dialogueManager;
    [Header("Ink JSON")]
    [SerializeField] TextAsset inkJSON;
    public bool showingText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        sentenceEndingPunctiation = new()
        {
            ".",
            "?",
            "!",
        };

        dialogueManager.EnterDialogueMode(inkJSON);
        ///StartCoroutine(DisplayPhraseInSyllables(testPhrase, timeBetweenSyllables, timeBetweenWords, timeBetweenSentences));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator DisplayPhraseInSyllables(string _phrase, string tag, float _timeBetweenSyllables, float _timeBetweenWords, float _timeBetweenSentences)
    {
        showingText = true;
        Debug.Log("Text tag = " + tag);
        fullText.text = "";
        alphaText.text = "";

        //Initialization
        List<string> words = GetWordsInPhrase(_phrase);
        bool capitalizeNextWord = true;

        //Run each word through the loop
        for(int i = 0; i < words.Count; i++)
        {
            string _word = words[i];
            //Don't end the sentence
            if(!sentenceEndingPunctiation.Contains(_word))
            {
                //Split the word into syllables
                List<string> syllables = SplitSyllables(_word, capitalizeNextWord).ToList();
                capitalizeNextWord = false;

                for(int i2 = 0; i2 < syllables.Count; i2++)
                {
                    //Wait and then display a new syllable
                    string _syllable = syllables[i2];
                    alphaText.text += _syllable;
                    StartCoroutine(FadeInText(fadeInTime));
                    if(i2 + 1 <= syllables.Count) yield return new WaitForSeconds(_timeBetweenSyllables);
                }

                //End of word, checks if theres another word, if so adds a space and waits
                if(!(words.Count <= i + 1))
                {
                    if(!sentenceEndingPunctiation.Contains(words[i+1])) 
                    {
                        alphaText.text += " ";
                        yield return new WaitForSeconds(_timeBetweenWords);
                    }
                }
            }
            //End the sentence based on punctuation
            else
            {
                alphaText.text += _word;
                alphaText.text += " ";
                StartCoroutine(FadeInText(fadeInTime));
                capitalizeNextWord = true;
                yield return new WaitForSeconds(_timeBetweenSentences);
            }
            
        }
        showingText = false;
    }

    IEnumerator FadeInText(float duration)
    {
        TextMeshProUGUI newAlphaInstance = Instantiate(alphaText, textHolder);
        //SetAlpha(0, newAlphaInstance);
        
        float startAlpha = newAlphaInstance.color.a;  // Get the current alpha value
        float elapsedTime = 0f;
        float targetAlpha = 1f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);
            SetAlpha(alpha, newAlphaInstance);
            yield return null;  // Wait for the next frame
        }

        // Ensure the target alpha is set at the end of the lerp
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
        List<string> words;
        words = _phrase.Split().ToList();
        return words;
    }

    public static string[] SplitSyllables(string word, bool firstWord)
    {
        Debug.Log($"Split Syllables Input: {word}, {firstWord}");

        if((firstWord || word == "i") && (word != "")) word = word.ToLower();
        string[] foundArray;

        if (!CMUDictLoader.Pronunciations.ContainsKey(word) && word.Length > 0) 
        {
            string removedChar = word[word.Length - 1].ToString();  // Get the last character
            word = word.Substring(0, word.Length - 1);
            if(!CMUDictLoader.Pronunciations.ContainsKey(word)) return new string[0];
            foundArray = CMUDictLoader.Pronunciations[word];
            foundArray[foundArray.Length - 1] += removedChar;
            if(firstWord || word == "i") foundArray[0] = char.ToUpper(foundArray[0][0]) + foundArray[0].Substring(1);
            return foundArray;
        }
        if(!CMUDictLoader.Pronunciations.ContainsKey(word)) return new string[0];
        
        foundArray = CMUDictLoader.Pronunciations[word];
        if((firstWord || word == "i") && (word != "")) foundArray[0] = char.ToUpper(foundArray[0][0]) + foundArray[0].Substring(1);

        Debug.Log($"Split Syllables Output: {foundArray.Length}");
        return foundArray;
    }
}
