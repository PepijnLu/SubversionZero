using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextManager textManager;
    public static GameManager instance;
    public bool inBoardView, isTransitioning;
    [SerializeField] Transform renderCamTransform;
    Color originalColor;
    Camera renderCam;
    [Header("Levels")]
    [SerializeField] GameObject levels;
    [SerializeField] Image levelTransitionImg;
    public int currentLevelIndex;
    void Awake()
    {
        instance = this;   
        renderCam = renderCamTransform.gameObject.GetComponent<Camera>();
        originalColor = renderCam.backgroundColor;
    }

    public IEnumerator TransitionLevel(int _newLevelIndex)
    {
        isTransitioning = true;
        StartCoroutine(LerpBackgroundColor(renderCam.backgroundColor, new Color(0, 0, 0, 0), 2, renderCam));
        yield return GenericFunctions.instance.FadeImage(levelTransitionImg, 2, 1);
        levels.transform.GetChild(currentLevelIndex).gameObject.SetActive(false);

        Transform newLevel = levels.transform.GetChild(_newLevelIndex); 
        newLevel.gameObject.SetActive(true);

        Transform newLevelCam = newLevel.GetChild(0);
        Light newLevelLight = newLevel.GetChild(1).GetComponent<Light>();

        renderCam.transform.position = newLevelCam.transform.position;

        StartCoroutine(GenericFunctions.instance.FadeImage(levelTransitionImg, 0, 0));
        yield return GenericFunctions.instance.FlickerLight(newLevelLight);

        yield return LerpBackgroundColor(renderCam.backgroundColor, originalColor, 2, renderCam);

        currentLevelIndex++;

        isTransitioning = false;
    }

    public IEnumerator LerpBackgroundColor(Color _startColor, Color _endColor, float _duration, Camera _camera)
    {
        float elapsed = 0f;
        float biggestColorDifference = 0;

        while (elapsed < _duration)
        {
            float t = elapsed / _duration;
            Color currentColor = _camera.backgroundColor;
            _camera.backgroundColor = Color.Lerp(_startColor, _endColor, t);

            float colorDifference = Mathf.Abs(currentColor.r - _camera.backgroundColor.r);

            if(colorDifference > biggestColorDifference) biggestColorDifference = colorDifference;

            elapsed += Time.deltaTime;
            yield return null;
        }
        Debug.Log($"Biggest color diff: {biggestColorDifference}");
        _camera.backgroundColor = _endColor;
    }
}
