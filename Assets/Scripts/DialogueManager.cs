using UnityEngine;
using Ink.Runtime;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class DialogueManager : MonoBehaviour
{
    Story currentStory;
    bool dialogueIsPlaying;
    [SerializeField] TextManager textManager;
    [SerializeField] List<TextMeshProUGUI> choicesTexts;
    string pattern = @"^(.*?)<([^<>]+)>$";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!dialogueIsPlaying) return;

        if(Input.GetKeyDown(KeyCode.Space) && !textManager.showingText)
        {
            ContinueStory();
        }
    }

    public void EnterDialogueMode(TextAsset _inkJson)
    {
        currentStory = new Story(_inkJson.text);
        dialogueIsPlaying = true;

        ContinueStory();
    }

    void ExitDialogueMode()
    {
        dialogueIsPlaying = false;
        //dialogueText.text = "";
    }

    void ContinueStory()
    {
        if(currentStory.canContinue)
        {
            string textFromJson = currentStory.Continue();

            string tag = GetTagFromString(textFromJson);
            string mainText = GetMainTextFromString(textFromJson);

            Debug.Log($"New main text: {mainText}");
            
            StartCoroutine(textManager.DisplayPhraseInSyllables(mainText, tag, textManager.timeBetweenSyllables, textManager.timeBetweenWords, textManager.timeBetweenSentences));
            DisplayChoices();
        }
        else
        {
            Debug.Log("Exiting dialogue mode");
            ExitDialogueMode();
        }
    }

    void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        //enable and initliaze the choices
        for(int i = 0; i < currentChoices.Count; i++)
        {
            Choice _choice = currentChoices[i];
            choicesTexts[i].transform.parent.gameObject.SetActive(true);
            choicesTexts[i].text = _choice.text;
        }

        //set the remaining choices in the UI to false

    }

    public void MakeChoice(int choiceIndex)
    {
        currentStory.ChooseChoiceIndex(choiceIndex);
        foreach(TextMeshProUGUI _txt in choicesTexts)
        {
            _txt.text = "";
            _txt.transform.parent.gameObject.SetActive(false);
        }
        ContinueStory();
    }

    string GetMainTextFromString(string _str)
    {
        Match match = Regex.Match(_str, pattern);

        if (match.Success)
        {
            string mainText = match.Groups[1].Value;
            Debug.Log("Main text: " + mainText);
            return mainText;
        }
        else
        {
            return _str;
        }
    }

    string GetTagFromString(string _str)
    {
        Match match = Regex.Match(_str, pattern);
        if (match.Success)
        {
            string tag = match.Groups[2].Value;
            Debug.Log("Tag: " + tag);
            return tag;
        }
        else
        {
            return "";
        }
    }
}
