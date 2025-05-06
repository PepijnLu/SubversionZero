using UnityEngine;
using UnityEngine.UI;
using Ink.Runtime;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class Character : MonoBehaviour
{
    [SerializeField] TextAsset storyTxt;
    public MeshRenderer meshRenderer;
    [SerializeField] DialogueManager dialogueManager;
    public Material defaultCharacterMaterial, hoverCharacterMaterial;
    [SerializeField] Sprite defaultDialogueSprite, happyDialogueSprite, sadDialogueSprite, angryDialogueSprite, surprisedDialogueSprite;
    public Dictionary<string, Sprite> characterSprites;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        characterSprites = new()
        {
            ["default"] = defaultDialogueSprite,
            ["happy"] = happyDialogueSprite,
            ["sad"] = sadDialogueSprite,
            ["angry"] = angryDialogueSprite,
            ["surprised"] = surprisedDialogueSprite
        };
    }

    public Character InitiateDialogue()
    {
        if(GameManager.instance.isTransitioning) return null;
        if(GameManager.instance.textManager.showingText) return null;

        dialogueManager.SwitchStory(this, storyTxt);
        return this;
    }

}
