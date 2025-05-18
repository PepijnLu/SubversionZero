using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public TextManager textManager;
    public static GameManager instance;
    public bool inBoardView, isTransitioning, inDialogue;
    [SerializeField] Transform renderCamTransform;
    Color originalColor;
    Camera renderCam;
    [Header("Levels")]
    [SerializeField] List<GameObject> levels;
    Dictionary<string, GameObject> levelNames;
    [SerializeField] Image levelTransitionImg;
    public string currentLevel;
    void Awake()
    {
        instance = this;   
        renderCam = renderCamTransform.gameObject.GetComponent<Camera>();
        originalColor = renderCam.backgroundColor;
    }

    void Start()
    {
        levelNames = new();
        foreach(GameObject _level in levels)
        {
            levelNames.Add(_level.name, _level);
        }
        currentLevel = "LivingRoom";

        Transform newLevel = levelNames["LivingRoom"].transform;
        newLevel.gameObject.SetActive(true);

        Camera newLevelCam = newLevel.GetChild(0).GetComponent<Camera>();

        renderCam.transform.position = newLevelCam.transform.position;
        renderCam.transform.rotation = newLevelCam.transform.rotation;
        renderCam.fieldOfView = newLevelCam.fieldOfView;
    }

    public IEnumerator TransitionLevel(string _connectingRooms)
    {
        isTransitioning = true;
        StartCoroutine(LerpBackgroundColor(renderCam.backgroundColor, new Color(0, 0, 0, 0), 2, renderCam));
        yield return GenericFunctions.instance.FadeImage(levelTransitionImg, 2, 1);


        //levels.transform.GetChild(currentLevelIndex).gameObject.SetActive(false);
        levelNames[currentLevel].SetActive(false);

        //Transform newLevel = levels.transform.GetChild(_newLevelIndex);
        string roomToMoveTo = GetRoomToMoveTo(_connectingRooms);
        Transform newLevel = levelNames[roomToMoveTo].transform;
        newLevel.gameObject.SetActive(true);

        Camera newLevelCam = newLevel.GetChild(0).GetComponent<Camera>();
        Light newLevelLight = newLevel.GetChild(1).GetComponent<Light>();

        renderCam.transform.position = newLevelCam.transform.position;
        renderCam.transform.rotation = newLevelCam.transform.rotation;
        renderCam.fieldOfView = newLevelCam.fieldOfView;

        StartCoroutine(GenericFunctions.instance.FadeImage(levelTransitionImg, 0, 0));
        yield return GenericFunctions.instance.FlickerLight(newLevelLight);

        //yield return LerpBackgroundColor(renderCam.backgroundColor, originalColor, 2, renderCam);

        isTransitioning = false;
        currentLevel = roomToMoveTo;
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

    string GetRoomToMoveTo(string _connectingRooms)
    {
        string[] rooms = _connectingRooms.Split(','); // Split by space

        foreach (string _room in rooms)
        {
            if(!_room.Contains(currentLevel)) return _room;
        }

        throw new Exception("Can't Transition");
    }
}
