using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PointAndClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] float transitionDuration, orthoSize, perspectiveFOV, perspectiveDistance, orthoDistance;
    [SerializeField] Light overheadLight;
    private bool isOrthographic = true;
    private bool isTransitioning, transitioned, pannedToZone;
    //bool panning;
    Vector2 renderTextCenter;
    [SerializeField] LayerMask inspectionAreaLayer;
    [SerializeField] Camera renderCam;
    [SerializeField] RawImage rawImage;
    RenderTexture renderTexture;
    float originalFov;
    Vector3 lastCamPos;
    Quaternion lastCamRot;

    [Header("Panning")]
    [SerializeField] float pannedZoom;
    [SerializeField] float pannedDistance;
    [SerializeField] float panningDuration;
    [SerializeField] GameObject empty;

    void Start()
    {
        SetupCamera();
    }

    void Update()
    {
        if(!transitioned) 
        {
            if(!GameManager.instance.inBoardView) FirstClick();
            return;
        }

        if(Input.GetKeyDown(KeyCode.Escape) && pannedToZone && !GameManager.instance.isTransitioning && !GameManager.instance.inBoardView)
        {
            StartCoroutine(PanToOriginalPosition());
        }
    }

    IEnumerator PanToOriginalPosition()
    {
        GameManager.instance.isTransitioning = true;

        StartCoroutine(GenericFunctions.instance.LerpRotation(renderCam.transform, lastCamRot, panningDuration));
        StartCoroutine(GenericFunctions.instance.LerpFov(originalFov, originalFov, panningDuration, renderCam));
        yield return StartCoroutine(GenericFunctions.instance.LerpTransform(renderCam.transform, lastCamPos, panningDuration));

        GameManager.instance.isTransitioning = false;
        pannedToZone = false;
    }

    IEnumerator PanToZone(RaycastHit _hit)
    {
        GameManager.instance.isTransitioning = true;

        lastCamPos = renderCam.transform.position;
        lastCamRot = renderCam.transform.rotation;

        GameObject targetBox = _hit.collider.gameObject;
        Vector3 targetPostion = targetBox.transform.position + (pannedDistance * targetBox.transform.forward);

        GameObject directioncheck = Instantiate(empty, targetPostion, renderCam.transform.rotation);
        directioncheck.transform.LookAt(targetBox.transform);
        Quaternion targetRotation = directioncheck.transform.rotation;
        
        BoxCollider boxCollider = targetBox.GetComponent<BoxCollider>();
        float newCameraPov = GetPannedCameraFov(boxCollider);

        StartCoroutine(GenericFunctions.instance.LerpRotation(renderCam.transform, targetRotation, panningDuration));
        StartCoroutine(GenericFunctions.instance.LerpFov(originalFov, newCameraPov, panningDuration, renderCam));
        yield return StartCoroutine(GenericFunctions.instance.LerpTransform(renderCam.transform, targetPostion, panningDuration));

        GameManager.instance.isTransitioning = false;
        pannedToZone = true;
    }

    float GetPannedCameraFov(BoxCollider box)
    {
        Bounds bounds = box.bounds;
        Vector3 objectCenter = bounds.center;

        float objectHeight = bounds.size.y;
        float objectWidth = bounds.size.x;

        float biggestConstraint = Mathf.Max(objectHeight, objectWidth);
        Debug.Log($"Biggest Constraint = {biggestConstraint}");

        float requiredFov = 42.86f * biggestConstraint - 27.52f;
        return requiredFov;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Click recorded");
        RectTransform rectTransform = rawImage.rectTransform;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Normalize to [0,1] within the RawImage
            Vector2 normalized = Rect.PointToNormalized(rectTransform.rect, localPoint);

            // Convert to texture pixel coordinates
            float texX = normalized.x * renderTexture.width;
            float texY = normalized.y * renderTexture.height;

            Vector2 textureCoord = new Vector2(texX, texY);

            //Draw needed rays
            RaycastHit cameraPanHit = RaycastTargetLayer(textureCoord, 100f, inspectionAreaLayer);
            if(cameraPanHit.collider != null && !pannedToZone && !GameManager.instance.isTransitioning && !GameManager.instance.inBoardView) StartCoroutine(PanToZone(cameraPanHit));
        }
    }

    void SetupCamera()
    {
        overheadLight.enabled = false;
        renderCam.orthographic = true;
        renderCam.orthographicSize = orthoSize;
        renderCam.transform.position = new Vector3(0, 1.6f, orthoDistance);
        renderCam.transform.rotation = Quaternion.Euler(0, 0, 0);
        renderTexture = renderCam.targetTexture;
        renderTextCenter = new Vector2(renderTexture.width / 2f, renderTexture.height / 2f);
        originalFov = renderCam.fieldOfView;
    }

    void FirstClick()
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

        float startFOV = renderCam.fieldOfView;
        float startSize = renderCam.orthographicSize;
        float endFOV = perspectiveFOV;
        float endSize = orthoSize;

        Vector3 startPos = renderCam.transform.position;
        Vector3 endPos = toOrthographic
            ? new Vector3(0, 0, orthoDistance)
            : new Vector3(0, 2, perspectiveDistance); // slight angled view

        Quaternion startRot = renderCam.transform.rotation;
        Quaternion endRot = toOrthographic
            ? Quaternion.Euler(0, 0, 0)
            : Quaternion.Euler(13, 0, 0); // editor-style view

        renderCam.orthographic = toOrthographic;

        while (time < transitionDuration)
        {
            float t = time / transitionDuration;

            if (toOrthographic)
            {
                renderCam.fieldOfView = Mathf.Lerp(startFOV, 1f, t);
                renderCam.transform.position = Vector3.Lerp(startPos, endPos, t);
                renderCam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }
            else
            {
                renderCam.orthographicSize = Mathf.Lerp(startSize, 0.01f, t);
                renderCam.transform.position = Vector3.Lerp(startPos, endPos, t);
                renderCam.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            }

            time += Time.deltaTime;
            yield return null;
        }


        if (toOrthographic)
        {
            renderCam.orthographicSize = orthoSize;
            renderCam.transform.position = endPos;
            renderCam.transform.rotation = endRot;
        }
        else
        {
            renderCam.fieldOfView = perspectiveFOV;
            renderCam.transform.position = endPos;
            renderCam.transform.rotation = endRot;
        }

        isOrthographic = toOrthographic;
        isTransitioning = false;
        transitioned = true;
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

    RaycastHit RaycastTargetLayer(Vector2 direction, float distance, LayerMask layerMask)
    {
        // Cast a ray from the camera's render texture
        Ray ray = renderCam.ScreenPointToRay(direction);
        RaycastHit hitTarget = new();

        // Visualize the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * distance, Color.cyan, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, distance, layerMask))
        {
            hitTarget = hit;
        }

        return hitTarget;
    }
}