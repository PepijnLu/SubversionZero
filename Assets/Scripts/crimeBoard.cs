using UnityEngine;

public class crimeBoard : MonoBehaviour
{
    public Color originalLighting = new Color(152f, 152f, 152f);
    public Color boardLighting = new Color(0f, 0f, 0f);
    public Camera ogCam, boardCam;


    void Start()
    {
        RenderSettings.ambientLight = originalLighting;
        Debug.Log("Ambient color changed to " + originalLighting);
        ogCam.enabled = true;
        boardCam.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            RenderSettings.ambientLight = boardLighting;
            ogCam.enabled = !ogCam.enabled;
            ogCam.depth = 0;
            boardCam.enabled = !boardCam.enabled;
            boardCam.depth = 5;
        }
    }
}
