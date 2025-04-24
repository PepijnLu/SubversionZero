using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public bool inBoardView, isTransitioning;
    [SerializeField] Transform renderCam;
    [Header("Levels")]
    [SerializeField] GameObject levels;
    [SerializeField] Image levelTransitionImg;
    public int currentLevelIndex;
    void Awake()
    {
        instance = this;   
    }

    public IEnumerator TransitionLevel(int _newLevelIndex)
    {
        isTransitioning = true;
        yield return GenericFunctions.instance.FadeImage(levelTransitionImg, 2, 1);
        levels.transform.GetChild(currentLevelIndex).gameObject.SetActive(false);

        Transform newLevel = levels.transform.GetChild(_newLevelIndex); 
        newLevel.gameObject.SetActive(true);

        Transform newLevelCam = newLevel.GetChild(0);
        Light newLevelLight = newLevel.GetChild(1).GetComponent<Light>();

        renderCam.transform.position = newLevelCam.transform.position;

        yield return GenericFunctions.instance.FadeImage(levelTransitionImg, 2, 0);

        currentLevelIndex++;

        yield return GenericFunctions.instance.FlickerLight(newLevelLight);

        isTransitioning = false;
    }
}
