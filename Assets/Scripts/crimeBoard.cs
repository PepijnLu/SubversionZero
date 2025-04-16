using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class crimeBoard : MonoBehaviour
{
    public Color originalLighting = new Color(152f, 152f, 152f);
    public Color boardLighting = new Color(0f, 0f, 0f);
    public Camera ogCam, boardCam;
    [SerializeField] RawImage stuffplayersees;
    bool showingBoard;
    public Light boardLight;

    void Start()
    {
        RenderSettings.ambientLight = originalLighting;
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
            boardLight.enabled = false;
            if (showingBoard)
            {
                ogCam.enabled = true;
                //stuffplayersees.texture = ogCam.targetTexture;
                showingBoard = false;
                boardCam.enabled = false;
                RenderSettings.ambientLight = originalLighting;
            }
            else
            {
                boardCam.enabled = true;
                //stuffplayersees.texture = boardCam.targetTexture;
                ogCam.enabled = false;
                showingBoard = true;
                RenderSettings.ambientLight = boardLighting;
                StartCoroutine(lightOn(boardLight));
            }
        }
    }

    IEnumerator lightOn(Light _light)
    {
        yield return new WaitForSeconds(0.1f);
        _light.enabled = true;
        yield return new WaitForSeconds(0.2f);
        _light.enabled = false;
        yield return new WaitForSeconds(0.4f);
        _light.enabled = true;
    }
}
