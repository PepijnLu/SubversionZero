using UnityEngine;
using Ink.Runtime;
using TMPro;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Drawing;

public class DialogueManager : MonoBehaviour
{
    Story currentStory;
    bool dialogueIsPlaying, displayingChoices;
    [SerializeField] TextManager textManager;
    [SerializeField] PointAndClick pointAndClick;
    [SerializeField] List<TextMeshProUGUI> choicesTexts;
    Dictionary<Character, Story> charactersStories = new();
    bool leftCharOnScreen, rightCharOnScreen;
    public Character currentLeftCharacter, currentRightCharacter;  
    string pattern = @"^(.*?)<([^<>]+)>$";
    [Header("Dialogue Scene Elements")]
    [SerializeField] float transitionTime;
    [SerializeField] Image textBox;
    [SerializeField] Image nameTag;
    [SerializeField] Image darkenImg;
    [SerializeField] Image leftCharacter;
    [SerializeField] Image rightCharacter;
    [SerializeField] Transform textBoxTargetTransform; 
    [SerializeField] Transform leftCharacterTransform; 
    [SerializeField] Transform rightCharacterTransform; 
    Vector3 offScreenTextBoxPosition, offscreenLeftCharacterPosition, offscreenRightCharacterPosition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        offScreenTextBoxPosition = textBox.transform.position;
        offscreenLeftCharacterPosition = leftCharacter.transform.position;
        offscreenRightCharacterPosition = rightCharacter.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if(!dialogueIsPlaying) return;

        if( (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && !textManager.showingText && !displayingChoices)
        {
            StartCoroutine(ContinueStory(true));
        }
    }

    public void SwitchStory(Character _character, TextAsset _txt)
    {
        nameTag.sprite = _character.nameTag;
        //Initiate story
        if(!charactersStories.ContainsKey(_character))
        {
            GameManager.instance.inDialogue = true;
            charactersStories.Add(_character, new Story(_txt.text));
            currentStory = charactersStories[_character];
            //Fade in to the right for now
            currentRightCharacter = _character;
            StartCoroutine(TransitionCharacter(true, true, false, true));
        }
        //Switch to another chars story
        else if(currentStory != charactersStories[_character])
        {
            GameManager.instance.inDialogue = true;
            //Fade in to the right for now
            currentRightCharacter = _character;
            StartCoroutine(TransitionCharacter(true, true, false, false));
            currentStory = charactersStories[_character];
        }
    }

    void ExitDialogueMode()
    {
        Debug.Log("Exiting dialogue mode");
        dialogueIsPlaying = false;
        pointAndClick.dialoguingChar.meshRenderer.material = pointAndClick.dialoguingChar.defaultCharacterMaterial;
        pointAndClick.dialoguingChar = null;
        GameManager.instance.inDialogue = false;
        textManager.DisableText();
    }

    IEnumerator ContinueStory(bool _dontAdvance, bool _showText = true)
    {
        dialogueIsPlaying = true;

        if(currentStory.canContinue || !_dontAdvance)
        {
            //textIsBeingWritten = true;
            string textFromJson;

            if(!_dontAdvance) textFromJson = currentStory.currentText;
            else textFromJson = currentStory.Continue();

            string tag = GetTagFromString(textFromJson);
            string mainText = GetMainTextFromString(textFromJson);

            HandleTag(tag);

            Debug.Log($"New main text: {mainText}");
            
            FModManager.instance.StartDialogue(currentRightCharacter);
            if(_showText) yield return StartCoroutine(textManager.DisplayPhraseInSyllables(mainText, tag, textManager.timeBetweenSyllables, textManager.timeBetweenWords, textManager.timeBetweenSentences));
            else StartCoroutine(ContinueStory(true));
            //textIsBeingWritten = false;
            DisplayChoices();
        }
        else if (currentStory.currentChoices.Count > 0)
        {
            DisplayChoices();
        }
        else
        {
            Debug.Log($"Transitioning character out: {currentStory.canContinue} , {_dontAdvance}");
            StartCoroutine(TransitionCharacter(false, true, false, false));
        }
        yield return null;
    }

    void HandleTag(string _tag)
    {
        //right character only for now
        if(!currentRightCharacter.characterSprites.ContainsKey(_tag)) _tag = "default";
        
        rightCharacter.sprite = currentRightCharacter.characterSprites[_tag];
        // switch(_tag)
        // {
        //     case "default":
                
        //         break;
        //     case "happy":

        //         break;
        //     case "sad":

        //         break;
        // }
    }

    void DisplayChoices()
    {
        List<Choice> currentChoices = currentStory.currentChoices;

        //enable and initliaze the choices
        for(int i = 0; i < currentChoices.Count; i++)
        {
            displayingChoices = true;
            GameManager.instance.displayingChoices = true;
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
        displayingChoices = false;
        GameManager.instance.displayingChoices = false;
        StartCoroutine(ContinueStory(true, false));
    }

    IEnumerator TransitionCharacter(bool _in, bool _right, bool _switch, bool _dontAdvance)
    {
        if(!_in) ExitDialogueMode();
        Vector3 characterSpriteTargetPos;

        if(_right)
        {
            characterSpriteTargetPos = rightCharacterTransform.position;
            if(_in) 
            {
                if(rightCharOnScreen)
                {
                    yield return StartCoroutine(TransitionCharacter(false, true, true, true));
                }

                rightCharOnScreen = true;
            }
        }
        else
        {
            characterSpriteTargetPos = leftCharacterTransform.position;
            if(_in) 
            {
                if(leftCharOnScreen)
                {
                    yield return StartCoroutine(TransitionCharacter(false, false, true, true));
                }

                leftCharOnScreen = true;
            }
        }

        if(_in)
        {   
            //Right for now
            rightCharacter.sprite = currentRightCharacter.characterSprites["default"];

            //Darken background
            if(!_switch) StartCoroutine(GenericFunctions.instance.FadeImage(darkenImg, .2f, .7f));
            //Fade in textbox
            if(!_switch) StartCoroutine(GenericFunctions.instance.LerpTransform(textBox.transform, textBoxTargetTransform.position, transitionTime));
            //Fade in character sprite
            yield return StartCoroutine(GenericFunctions.instance.LerpTransform(rightCharacter.transform, characterSpriteTargetPos, transitionTime));

            StartCoroutine(ContinueStory(_dontAdvance));
        }
        else
        {
            //Undarken background
            if(!_switch) StartCoroutine(GenericFunctions.instance.FadeImage(darkenImg, .2f, 0f));
            //Fade out textbox
            if(!_switch) StartCoroutine(GenericFunctions.instance.LerpTransform(textBox.transform, offScreenTextBoxPosition, transitionTime));
            //Fade out character sprite
            yield return StartCoroutine(GenericFunctions.instance.LerpTransform(rightCharacter.transform, offscreenRightCharacterPosition, transitionTime));

            if(_right) rightCharOnScreen = false;
            else leftCharOnScreen = false;
        }
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
