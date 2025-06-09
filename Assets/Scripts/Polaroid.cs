using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SubversionZero.Audio;
using System.Linq;

public class Polaroid : MonoBehaviour
{
    public CapturableObject capturedObject;
    [SerializeField] RectTransform polaroidHoverDet, textHoverDet;
    [SerializeField] TextMeshProUGUI imageDescription, keyword;
    [SerializeField] GameObject description;
    public Image polaroidImage;
    bool showingDesc;
    string originalText;
    string[] originalWords;
    public List<int> indexesOfBoldedWords = new();

    public void CustomStart(CapturableObject _capturedObj, RawImage _renderTextureDisplay, Camera _renderCamera)
    {
        capturedObject = _capturedObj;
        originalText = _capturedObj.objectDescription;

        originalWords = originalText.Split(' ');
        List<string> boldedWords = new();

        for(int i = 0; i < originalWords.Length; i++)
        {
            Debug.Log("original word: " + originalWords[i]);
            string boldedWord = GetBoldedString(originalWords[i], i);
            boldedWords.Add(boldedWord);
        }

        // foreach (string word in originalWords)
        // {
        //     Debug.Log("original word: " + word);
        //     string boldedWord = GetBoldedString(word);
        //     boldedWords.Add(boldedWord);
        // }

        string boldedDescirption = string.Join(" ", boldedWords);

        imageDescription.text = boldedDescirption;
    }

    public void HandleShowingDescription(bool _overPolaroid)
    {
        if(_overPolaroid)
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
            showingDesc = false;
            description.SetActive(false);
        }
    }

    public void SetWord(string _text)
    {
        keyword.text = _text;
    }

    string GetBoldedString(string _input, int _index)
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
                indexesOfBoldedWords.Add(_index);
            }
        }

        return _input;
    }
}
