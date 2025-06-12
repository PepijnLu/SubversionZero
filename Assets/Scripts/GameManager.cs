using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SubversionZero.Audio;

public class GameManager : MonoBehaviour
{
    public TextManager textManager;
    public static GameManager instance;
    public bool inBoardView, isTransitioning, inDialogue, placingPin, displayingChoices;
    [SerializeField] Transform renderCamTransform;
    Color originalColor;
    Camera renderCam;
    [Header("Levels")]
    [SerializeField] List<GameObject> levels;
    Dictionary<string, GameObject> levelNames;
    [SerializeField] Image levelTransitionImg;
    public string currentLevel;
    public int succesfulConnections;

    public Camera RenderCam => renderCam;
    public Transform RenderCamTransform => renderCamTransform;
    void Awake()
    {
        instance = this;   
        renderCam = renderCamTransform.gameObject.GetComponent<Camera>();
        originalColor = renderCam.backgroundColor;
    }

    void Start()
    {
        levelNames = new();
        foreach (GameObject _level in levels)
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
        FModManager.instance.EnterRoom(currentLevel);
    }

    public IEnumerator TransitionLevel(string _connectingRooms)
    {
        isTransitioning = true;
        FModManager.instance.PlaySfx(SfxKey.Door);
        StartCoroutine(LerpBackgroundColor(renderCam.backgroundColor, new Color(0, 0, 0, 0), 2, renderCam));
        yield return GenericFunctions.instance.FadeImage(levelTransitionImg, 2, 1);


        //levels.transform.GetChild(currentLevelIndex).gameObject.SetActive(false);
        levelNames[currentLevel].SetActive(false);

        //Transform newLevel = levels.transform.GetChild(_newLevelIndex);
        string roomToMoveTo = GetRoomToMoveTo(_connectingRooms);
        Transform newLevel = levelNames[roomToMoveTo].transform;
        newLevel.gameObject.SetActive(true);

        Camera newLevelCam = newLevel.GetChild(0).GetComponent<Camera>();
        renderCam.transform.position = newLevelCam.transform.position;
        renderCam.transform.rotation = newLevelCam.transform.rotation;
        renderCam.fieldOfView = newLevelCam.fieldOfView;

        //Light newLevelLight = newLevel.GetChild(1).GetComponent<Light>();
        for (int i = 0; i < newLevel.childCount; i++)
        {
            GameObject child = newLevel.GetChild(i).gameObject;
            if ((child.GetComponent<Light>() != null) || (child.GetComponent<Character>() != null))
            {
                child.SetActive(false);
            }
        }

        StartCoroutine(GenericFunctions.instance.FadeImage(levelTransitionImg, 0, 0));
        for (int i = 0; i < newLevel.childCount; i++)
        {
            GameObject child = newLevel.GetChild(i).gameObject;
            if ((child.GetComponent<Light>() != null) || (child.GetComponent<Character>() != null))
            {
                StartCoroutine(GenericFunctions.instance.FlickerObject(child));
            }
            Debug.Log(child.name);
        }


        yield return new WaitForSeconds(0.4f);

        //yield return LerpBackgroundColor(renderCam.backgroundColor, originalColor, 2, renderCam);

        isTransitioning = false;
        currentLevel = roomToMoveTo;
        FModManager.instance.EnterRoom(roomToMoveTo);
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
