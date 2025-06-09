using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using SubversionZero.Audio;

public class Polaroid : MonoBehaviour, IPointerClickHandler
{
    public CapturableObject capturedObject;
    [SerializeField] RectTransform polaroidHoverDet, textHoverDet;
    [SerializeField] TextMeshProUGUI imageDescription, keyword;
    [SerializeField] GameObject description;
    bool showingDesc;
    string originalText;
    string[] originalWords;

    public void CustomStart(CapturableObject _capturedObj)
    {
        capturedObject = _capturedObj;
        originalText = _capturedObj.objectDescription;

        originalWords = originalText.Split(' ');
        List<string> boldedWords = new();

        foreach (string word in originalWords)
        {
            Debug.Log("original word: " + word);
            string boldedWord = GetBoldedString(word);
            boldedWords.Add(boldedWord);
        }

        string boldedDescirption = string.Join(" ", boldedWords);

        imageDescription.text = boldedDescirption;
    }
    void Update()
    {
        bool overPolaroid = RectTransformUtility.RectangleContainsScreenPoint(polaroidHoverDet, Input.mousePosition);

        if(overPolaroid)
        {
            if(!showingDesc) 
            {
                FModManager.instance.PlaySfx(SfxKey.PolaroidHover);
                showingDesc = true;
                description.SetActive(true);
            }
        }
        else if(showingDesc)
        {
            bool overText = RectTransformUtility.RectangleContainsScreenPoint(textHoverDet, Input.mousePosition);

            if(!overText)
            {
                showingDesc = false;
                description.SetActive(false);
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int wordIndex = TMP_TextUtilities.FindIntersectingWord(imageDescription, Input.mousePosition, null);

        if (wordIndex != -1)
        {
            TMP_WordInfo wordInfo = imageDescription.textInfo.wordInfo[wordIndex];
            string clickedWord = wordInfo.GetWord();

            Debug.Log("You clicked the word: " + clickedWord);

            foreach(string _word in originalWords)
            {
                if(_word.Contains("0" + clickedWord + "0"))
                {
                    keyword.text = clickedWord;
                    break;
                }
            }  
        }
    }

    string GetBoldedString(string _input)
    {
        int firstIndex = _input.IndexOf('0');

        if (firstIndex != -1)
        {
            // Replace the first '0' with <b>
            _input = _input.Remove(firstIndex, 1).Insert(firstIndex, "<b>");

            int secondIndex = _input.IndexOf('0', firstIndex + 3); // +3 because "<b>" is 3 chars longer than '0'
            if (secondIndex != -1)
            {
                _input = _input.Remove(secondIndex, 1).Insert(secondIndex, "</b>");
            }
        }

        return _input;
    }
}
