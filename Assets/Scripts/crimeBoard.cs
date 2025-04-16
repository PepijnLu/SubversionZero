using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CrimeBoard : MonoBehaviour
{
    public Color originalLighting = new Color(152f, 152f, 152f);
    public Color boardLighting = new Color(0f, 0f, 0f);
    public Camera ogCam, boardCam;
    [SerializeField] RawImage stuffplayersees;
    bool showingBoard, swappingView;
    public Light boardLight;
    [SerializeField] Image bottomEyelid, topEyelid;
    [SerializeField] float eyelidDistance, eyelidTime, delayAfterClosing, fadeDuration;

    void Start()
    {
        //RenderSettings.ambientLight = originalLighting;
        Debug.Log("Ambient color changed to " + originalLighting);
        ogCam.enabled = true;
        boardCam.enabled = false;
    }
    //RenderSettings.ambientLight = boardLighting;
    //ogCam.enabled = !ogCam.enabled;
    //        ogCam.depth = 0;
    //        boardCam.enabled = !boardCam.enabled;
    //        boardCam.depth = 5;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(!swappingView) StartCoroutine(SwapView(showingBoard));
        }
    }

    IEnumerator SwapView(bool _showingBoard)
    {
        swappingView = true;
        //boardLight.enabled = false;
    //Board Startup
        //Disable 
        if (_showingBoard)
        {
            //Fade in eyelids
            Color alpha0 = bottomEyelid.color;
            alpha0.a = 0;
            bottomEyelid.color = alpha0;
            topEyelid.color = alpha0;

            bottomEyelid.gameObject.SetActive(true);
            topEyelid.gameObject.SetActive(true);

            StartCoroutine(FadeImage(bottomEyelid, fadeDuration, 1));
            yield return FadeImage(topEyelid, fadeDuration, 1);

            ogCam.enabled = true;
            showingBoard = false;
            boardCam.enabled = false;
            //RenderSettings.ambientLight = originalLighting;
            yield return OpenCloseEyes(true);
        }
        //Enable
        else
        {
            yield return OpenCloseEyes(false);
            boardCam.enabled = true;
            ogCam.enabled = false;
            showingBoard = true;
            RenderSettings.ambientLight = boardLighting;

            yield return new WaitForSeconds(delayAfterClosing);
            bottomEyelid.gameObject.SetActive(false);
            topEyelid.gameObject.SetActive(false);

            //Flicker board light
            yield return new WaitForSeconds(0.1f);
            boardLight.enabled = true;
            yield return new WaitForSeconds(0.2f);
            boardLight.enabled = false;
            yield return new WaitForSeconds(0.4f);
            boardLight.enabled = true;
        }

        swappingView = false;
    }

    IEnumerator OpenCloseEyes(bool _open)
    {
        Vector3 bottomLidTargetPos = bottomEyelid.transform.position;
        Vector3 topLidTargetPos = topEyelid.transform.position;

        if(_open)
        {
            //Open eyelids
            bottomLidTargetPos.y -= eyelidDistance;
            topLidTargetPos.y += eyelidDistance;
        }
        else
        {
            bottomLidTargetPos.y += eyelidDistance;
            topLidTargetPos.y -= eyelidDistance;
        }

        StartCoroutine(LerpTransform(bottomEyelid.transform, bottomLidTargetPos, eyelidTime));
        yield return LerpTransform(topEyelid.transform, topLidTargetPos, eyelidTime);
    }

    IEnumerator FadeImage(Image _image, float _duration, float _target)
    {
        float currentValue = _image.color.a;
        float _elapsedTime = 0;
        Color color = _image.color;

        while (_elapsedTime <= _duration)
        {
            currentValue = Mathf.Lerp(currentValue, _target, _elapsedTime / _duration);
            color.a = currentValue;
            Debug.Log($"Changed {_image.name} alpha to {_image.color.a}");
            _image.color = color;
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = _target;
        _image.color = color;
    }

    IEnumerator LerpTransform(Transform _transform, Vector3 _targetPos, float _duration)
    {
        Vector3 startPos = _transform.position;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            _transform.position = Vector3.Lerp(startPos, _targetPos, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _transform.position = _targetPos;
    }
}
