using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;

public class PointAndClick : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] float transitionDuration, orthoSize, perspectiveFOV, perspectiveDistance, orthoDistance;
    [SerializeField] Light overheadLight;
    private bool isOrthographic = true;
    private bool isTransitioning, transitioned, pannedToZone, inPhotoMode;
    //bool panning;
    Vector2 renderTextCenter;
    [SerializeField] LayerMask inspectionAreaLayer;
    [SerializeField] Camera renderCam;
    [SerializeField] RawImage rawImage;
    RenderTexture renderTexture;
    float originalFov, originalFlashAlpha;
    Vector3 lastCamPos;
    Quaternion lastCamRot;

    [Header("Panning")]
    [SerializeField] float pannedZoom;
    [SerializeField] float pannedDistance;
    [SerializeField] float panningDuration;
    [SerializeField] GameObject empty;
    [Header("Polaraids")]
    [SerializeField] GameObject polaroidCam, polaroidPrefab, pictureLocations;
    [SerializeField] Image cameraFlashImg;
    [SerializeField] AudioSource flashSfx;
    [SerializeField] Transform boardTransform;
    [SerializeField] int captureSize;
    [SerializeField] LayerMask capturableLayer;
    List<GameObject> picturedObjects = new();
    [SerializeField] float flashFadeTime;
    int picturesTaken;
    bool takingPicture;


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

        if(Input.GetKeyDown(KeyCode.P))
        {
            SwitchPhotoMode();
        }

    }

    void SwitchPhotoMode()
    {
        if(GameManager.instance.isTransitioning) return;

        if(inPhotoMode)
        {
            inPhotoMode = false;
            polaroidCam.SetActive(false);
        }
        else
        {
            inPhotoMode = true;
            polaroidCam.SetActive(true);
        }
    }

    void TakePicture(RaycastHit _hit)
    {
        if(takingPicture) return;
        if(_hit.collider == null) return;
        if(picturedObjects.Contains(_hit.collider.gameObject)) return;

        takingPicture = true;
        GameObject hitObj = _hit.collider.gameObject;
        int layer = hitObj.layer;
        string layerName = LayerMask.LayerToName(layer);

        Debug.Log($"Hit object: {hitObj.name} on layer: {layerName} ({layer})");

        // Check if it's in the desired layer mask
        if (((1 << layer) & capturableLayer) != 0)
        {
            Debug.Log("Hit correct layer!");

            // --- Read from RenderTexture here like before ---
            RenderTexture currentRT = RenderTexture.active;
            RenderTexture.active = renderTexture;

            Vector3 screenPoint = Input.mousePosition;

            float rtX = (screenPoint.x / Screen.width) * renderTexture.width;
            float rtY = (screenPoint.y / Screen.height) * renderTexture.height;

            int x = Mathf.Clamp((int)rtX - captureSize / 2, 0, renderTexture.width - captureSize);
            int y = Mathf.Clamp((int)rtY - captureSize / 2, 0, renderTexture.height - captureSize);

            Texture2D texture = new Texture2D(captureSize, captureSize, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(x, y, captureSize, captureSize), 0, 0);
            texture.Apply();

            RenderTexture.active = currentRT;
            Debug.Log("Texture captured around mouse!");
            picturedObjects.Add(_hit.collider.gameObject);
            CreatePolaroid(texture);
        }
    }

    void CreatePolaroid(Texture2D _polaroid)
    {
        // Get pixels from your captured texture
        Color[] pixels = _polaroid.GetPixels();

        float brightnessMultiplier = 3; // Increase to make brighter (e.g. 1.5 = +50%)

        // Modify each pixel
        for (int i = 0; i < pixels.Length; i++)
        {
            Color c = pixels[i];
            c *= brightnessMultiplier;
            c.a = pixels[i].a; // Preserve alpha
            pixels[i] = c;
        }

        // Set modified pixels
        _polaroid.SetPixels(pixels);
        _polaroid.Apply();

        GameObject polaroid = Instantiate(polaroidPrefab, pictureLocations.transform.GetChild(picturesTaken).position, Quaternion.identity, boardTransform);
        picturesTaken++;
        RawImage polaroidImage = polaroid.transform.GetChild(1).GetComponent<RawImage>();
        polaroidImage.texture = _polaroid;
        StartCoroutine(CameraFlashEffect());
    }

    IEnumerator CameraFlashEffect()
    {
        yield return GenericFunctions.instance.FadeImage(cameraFlashImg, 0, originalFlashAlpha);
        flashSfx.Play();
        cameraFlashImg.gameObject.SetActive(true);
        yield return GenericFunctions.instance.FadeImage(cameraFlashImg, flashFadeTime, 0);
        cameraFlashImg.gameObject.SetActive(false);
        takingPicture = false;
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
        if(GameManager.instance.isTransitioning) return;
        
        Debug.Log("Click recorded");
        RectTransform rectTransform = rawImage.rectTransform;
        Vector2 localPoint;
        LayerMask targetLayer;

        if(inPhotoMode) targetLayer = capturableLayer;
        else targetLayer = inspectionAreaLayer;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
        {
            // Normalize to [0,1] within the RawImage
            Vector2 normalized = Rect.PointToNormalized(rectTransform.rect, localPoint);

            // Convert to texture pixel coordinates
            float texX = normalized.x * renderTexture.width;
            float texY = normalized.y * renderTexture.height;

            Vector2 textureCoord = new Vector2(texX, texY);

            //Draw needed rays
            Debug.Log($"Raycasting to {targetLayer.value}");
            RaycastHit cameraPanHit = RaycastTargetLayer(textureCoord, 100f, targetLayer);

            if(inPhotoMode && !GameManager.instance.isTransitioning) TakePicture(cameraPanHit);
            else if(cameraPanHit.collider != null && !pannedToZone && !GameManager.instance.isTransitioning && !GameManager.instance.inBoardView) StartCoroutine(PanToZone(cameraPanHit));
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
        originalFlashAlpha = cameraFlashImg.color.a;
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