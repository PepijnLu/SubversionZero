using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class CrimeBoard : MonoBehaviour
{
    public Color originalLighting = new Color(152f, 152f, 152f);
    public Color boardLighting = new Color(0f, 0f, 0f);
    public Camera ogCam, boardCam;
    [SerializeField] RawImage stuffplayersees;
    //bool GameManager.instance.isTransitioning;
    public Light boardLight;
    [SerializeField] Image bottomEyelid, topEyelid;
    [SerializeField] float eyelidDistance, eyelidTime, delayAfterClosing, fadeDuration;
    [SerializeField] GameObject board;
    [SerializeField] Image canvasLighting;

    void Start()
    {
        //RenderSettings.ambientLight = originalLighting;
        Debug.Log("Ambient color changed to " + originalLighting);
        ogCam.enabled = true;
        boardCam.enabled = false;
        //RenderSettings.ambientLight = boardLighting;
    }
    //ogCam.enabled = !ogCam.enabled;
    //        ogCam.depth = 0;
    //        boardCam.enabled = !boardCam.enabled;
    //        boardCam.depth = 5;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if(!GameManager.instance.isTransitioning && !GameManager.instance.inDialogue) StartCoroutine(SwapView(GameManager.instance.inBoardView));
        }
    }

    IEnumerator SwapView(bool _showingBoard)
    {
        GameManager.instance.isTransitioning = true;
        //boardLight.enabled = false;
    //Board Startup
        //Disable 
        if (_showingBoard)
        {
            //Fade in eyelids
            FModManager.instance.StopPinboard();
            Color alpha0 = bottomEyelid.color;
            alpha0.a = 0;
            bottomEyelid.color = alpha0;
            topEyelid.color = alpha0;
            canvasLighting.color = alpha0;

            canvasLighting.gameObject.SetActive(true);
            bottomEyelid.gameObject.SetActive(true);
            topEyelid.gameObject.SetActive(true);

            StartCoroutine(GenericFunctions.instance.FadeImage(bottomEyelid, fadeDuration, 1));
            StartCoroutine(GenericFunctions.instance.FadeImage(canvasLighting, fadeDuration, 1));
            yield return GenericFunctions.instance.FadeImage(topEyelid, fadeDuration, 1);

            ogCam.enabled = true;
            GameManager.instance.inBoardView = false;
            boardCam.enabled = false;
            board.SetActive(false);
            canvasLighting.gameObject.SetActive(false);
            //RenderSettings.ambientLight = originalLighting;
            yield return OpenCloseEyes(true);
        }
        //Enable
        else
        {
            StartCoroutine(GenericFunctions.instance.FadeImage(canvasLighting, 0, 1));
            canvasLighting.gameObject.SetActive(true);
            FModManager.instance.StartPinboard();
            yield return OpenCloseEyes(false);
            boardCam.enabled = true;
            ogCam.enabled = false;
            board.SetActive(true);
            GameManager.instance.inBoardView = true;
            RenderSettings.ambientLight = boardLighting;

            yield return new WaitForSeconds(delayAfterClosing);
            bottomEyelid.gameObject.SetActive(false);
            topEyelid.gameObject.SetActive(false);

            //Flicker board light
            yield return new WaitForSeconds(0.1f);
            boardLight.enabled = true;
            canvasLighting.gameObject.SetActive(false);
            yield return new WaitForSeconds(0.2f);
            boardLight.enabled = false;
            canvasLighting.gameObject.SetActive(true);
            yield return new WaitForSeconds(0.4f);
            boardLight.enabled = true;
            canvasLighting.gameObject.SetActive(false);
        }

        GameManager.instance.isTransitioning = false;
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

        StartCoroutine(GenericFunctions.instance.LerpTransform(bottomEyelid.transform, bottomLidTargetPos, eyelidTime));
        yield return GenericFunctions.instance.LerpTransform(topEyelid.transform, topLidTargetPos, eyelidTime);
    }
}
