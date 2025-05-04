using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    [SerializeField] TextAsset storyTxt;
    [SerializeField] DialogueManager dialogueManager;
    [SerializeField] Sprite defaultCharacterSprite, happyCharacterSprite, sadCharacterSprite, angryCharacterSprite, surprisedCharacterSprite;
    public Dictionary<string, Sprite> characterSprites;

    void Start()
    {
        characterSprites = new()
        {
            ["default"] = defaultCharacterSprite,
            ["happy"] = happyCharacterSprite,
            ["sad"] = sadCharacterSprite,
            ["angry"] = angryCharacterSprite,
            ["surprised"] = surprisedCharacterSprite
        };
    }

    public void InitiateDialogue()
    {
        if(GameManager.instance.isTransitioning) return;
        if(GameManager.instance.textManager.showingText) return;

        dialogueManager.SwitchStory(this, storyTxt);
    }
}
