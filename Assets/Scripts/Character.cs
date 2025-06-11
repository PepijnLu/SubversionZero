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
    [SerializeField] Sprite defaultDialogueSprite, happyDialogueSprite, sadDialogueSprite, angryDialogueSprite, scaredDialogueSprite;
    public Dictionary<string, Sprite> characterSprites;
    public Image charImg;
    public Sprite defaultSprite, hoverSprite;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        characterSprites = new()
        {
            ["default"] = defaultDialogueSprite,
            ["happy"] = happyDialogueSprite,
            ["sad"] = sadDialogueSprite,
            ["angry"] = angryDialogueSprite,
            ["scared"] = scaredDialogueSprite
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
