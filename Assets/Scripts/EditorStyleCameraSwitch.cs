using UnityEngine;
using System.Collections;

public class EditorStyleCameraSwitch : MonoBehaviour
{
    public float transitionDuration = 1f;
    public float orthoSize = 5f;
    public float perspectiveFOV = 60f;
    public float perspectiveDistance = -5f;
    public float orthoDistance = 1f;
    public Light overheadLight;

    private Camera cam;
    private bool isOrthographic = true;
    private bool isTransitioning = false;

    void Start()
    {
        overheadLight.enabled = false;
        cam = GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = orthoSize;
        cam.transform.position = new Vector3(0, 1.6f, orthoDistance);
        cam.transform.rotation = Quaternion.Euler(0, 0, 0);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isTransitioning)
        {
            StartCoroutine(SwitchProjection(!isOrthographic));
            StartCoroutine(Flicker());
        }
    }

    IEnumerator SwitchProjection(bool toOrthographic)
    {
        isTransitioning = true;

        float time = 0f;

        float startFOV = cam.fieldOfView;
        float startSize = cam.orthographicSize;
        float endFOV = perspectiveFOV;
        float endSize = orthoSize;

        Vector3 startPos = cam.transform.position;
        Vector3 endPos = toOrthographic
            ? new Vector3(0, 0, orthoDistance)
            : new Vector3(0, 2, perspectiveDistance); // slight angled view

        Quaternion startRot = cam.transform.rotation;
        Quaternion endRot = toOrthographic
            ? Quaternion.Euler(0, 0, 0)
            : Quaternion.Euler(13, 0, 0); // editor-style view

        cam.orthographic = toOrthographic;

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;

            if (toOrthographic)
            {
                cam.fieldOfView = Mathf.Lerp(startFOV, 1f, t);
                cam.transform.position = Vector3.Lerp(startPos, endPos, t);
                cam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            else
            {
                cam.orthographicSize = Mathf.Lerp(startSize, 0.01f, t);
                cam.transform.position = Vector3.Lerp(startPos, endPos, t);
                cam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }

            time += Time.deltaTime;
            yield return null;
        }


        if (toOrthographic)
        {
            cam.orthographicSize = orthoSize;
            cam.transform.position = endPos;
            cam.transform.rotation = endRot;
        }
        else
        {
            cam.fieldOfView = perspectiveFOV;
            cam.transform.position = endPos;
            cam.transform.rotation = endRot;
        }

        isOrthographic = toOrthographic;
        isTransitioning = false;
    }

    IEnumerator Flicker() 
    {
        yield return new WaitForSeconds(0.5f);
        overheadLight.enabled = true;
        yield return new WaitForSeconds(0.1f);
        overheadLight.enabled = false;
        yield return new WaitForSeconds(0.5f);
        overheadLight.enabled = true;
    }
}