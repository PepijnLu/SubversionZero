using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using FMODUnity;
using System.Collections.Generic;
using Unity.VisualScripting;
using SubversionZero.Audio;
using GogoGaga.OptimizedRopesAndCables;

public class PointAndClick : MonoBehaviour
{
    [SerializeField] float transitionDuration, orthoSize, perspectiveFOV, perspectiveDistance, orthoDistance;
    [SerializeField] Light overheadLight;
    private bool isOrthographic = true;
    private bool isTransitioning, transitioned, pannedToZone, inPhotoMode;
    [SerializeField] Camera renderCam;
    [SerializeField] RawImage rawImage;
    RenderTexture renderTexture;
    float originalFov, originalFlashAlpha;
    Vector3 lastCamPos;
    Quaternion lastCamRot;
    GameObject lastHoveringChar;
    Polaroid lastPolaroid;
    public Character hoveringChar, dialoguingChar;
    [Header("LayerMasks")]
    [SerializeField] Camera normalRaycastCam;
    [SerializeField] Camera boardRaycastCam;
    [SerializeField] LayerMask inspectionAreaLayer;
    [SerializeField] LayerMask capturableLayer;
    [SerializeField] LayerMask doorLayer;
    [SerializeField] LayerMask characterLayer;
    [SerializeField] LayerMask pinLayer;
    [SerializeField] LayerMask polaroidHoverLayer;
    [SerializeField] LayerMask wordLayer;


    [Header("Panning")]
    [SerializeField] float pannedZoom;
    [SerializeField] float pannedDistance;
    [SerializeField] float panningDuration;
    [Header("Polaraids")]
    [SerializeField] GameObject polaroidCam, pictureLocations, pictureTakenUI;
    [SerializeField] Polaroid polaroidPrefab;
    [SerializeField] Image cameraFlashImg;
    [SerializeField] StudioEventEmitter flashSfx;
    [SerializeField] Transform boardTransform;
    [SerializeField] int captureSize;
    List<GameObject> picturedObjects = new();
    [SerializeField] float flashFadeTime;
    [SerializeField] GameObject pin;
    [SerializeField] Rope thread;
    int picturesTaken;
    bool takingPicture;
    bool placingPin;
    GameObject originPin, pinToBePlaced, otherPin;
    Rope ropeToBePlaced;


    void Start()
    {
        SetupCamera();
        transitioned = true;
        lastHoveringChar = gameObject;
        //Example of how to set an emitters parameter
        ///FModManager.instance.SetParameter(flashSfx, "Surface Type", 1);
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

        if(Input.GetKeyDown(KeyCode.P) && !GameManager.instance.isTransitioning && !GameManager.instance.inDialogue)
        {
            SwitchPhotoMode();
        }

        if(Input.GetMouseButtonDown(0) && hoveringChar != null) 
        {
            Character tempChar = hoveringChar.InitiateDialogue();
            if(tempChar != null) dialoguingChar = tempChar;
        }


        InitiateRaycast();
    }

    void SwitchPhotoMode()
    {
        if(GameManager.instance.isTransitioning) return;

        if(inPhotoMode)
        {
            inPhotoMode = false;
            polaroidCam.SetActive(false);
            // Play sound for putting camera away
            FModManager.instance.PlaySfx(SfxKey.PolaroidAway);
        }
        else
        {
            inPhotoMode = true;
            polaroidCam.SetActive(true);
            // Play sound for equipping the camera
        FModManager.instance.PlaySfx(SfxKey.PolaroidGrab);
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
        // Play sound for taking a picture
        FModManager.instance.PlaySfx(SfxKey.PolaroidPic);

        //Debug.Log($"Hit object: {hitObj.name} on layer: {layerName} ({layer})");

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

            CreatePolaroid(texture, _hit.collider.gameObject.GetComponent<CapturableObject>());
        }
    }

    void CreatePolaroid(Texture2D _polaroid, CapturableObject _capturableObject)
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

        Polaroid polaroid = Instantiate(polaroidPrefab, pictureLocations.transform.GetChild(picturesTaken).position, Quaternion.Euler(0, 180, 0), boardTransform);
        polaroid.CustomStart(_capturableObject, rawImage, renderCam);
        picturesTaken++;
        //RawImage polaroidImage = polaroid.transform.GetChild(1).GetComponent<RawImage>();
        //polaroidImage.texture = _polaroid;
        Image polaroidImage = polaroid.polaroidImage;
        polaroidImage.sprite = _capturableObject.objectPicture;
        StartCoroutine(CameraFlashEffect());
        StartCoroutine(PictureTakenUI());
    }
    IEnumerator CameraFlashEffect()
    {
        yield return GenericFunctions.instance.FadeImage(cameraFlashImg, 0, originalFlashAlpha);
        cameraFlashImg.gameObject.SetActive(true);
        yield return GenericFunctions.instance.FadeImage(cameraFlashImg, flashFadeTime, 0);
        cameraFlashImg.gameObject.SetActive(false);
        takingPicture = false;
    }

    IEnumerator PictureTakenUI()
    {
        pictureTakenUI.SetActive(true);
        yield return new WaitForSeconds(2f);
        pictureTakenUI.SetActive(false);
    }

    IEnumerator PanToOriginalPosition()
    {
        GameManager.instance.isTransitioning = true;
        float startFov = renderCam.fieldOfView;

        StartCoroutine(GenericFunctions.instance.LerpRotation(renderCam.transform, lastCamRot, panningDuration));
        StartCoroutine(GenericFunctions.instance.LerpFov(startFov, originalFov, panningDuration, renderCam));
        yield return StartCoroutine(GenericFunctions.instance.LerpTransform(renderCam.transform, lastCamPos, panningDuration));

        GameManager.instance.isTransitioning = false;
        pannedToZone = false;
    }

    IEnumerator PanToZone(RaycastHit _hit)
    {
        Camera targetCam = _hit.collider.transform.GetChild(0).GetComponent<Camera>();

        GameManager.instance.isTransitioning = true;

        lastCamPos = renderCam.transform.position;
        lastCamRot = renderCam.transform.rotation;

        Vector3 targetPostion = targetCam.transform.position;
        Quaternion targetRotation = targetCam.transform.rotation;
        float targetPov = targetCam.fieldOfView;

        StartCoroutine(GenericFunctions.instance.LerpRotation(renderCam.transform, targetRotation, panningDuration));
        StartCoroutine(GenericFunctions.instance.LerpFov(originalFov, targetPov, panningDuration, renderCam));
        yield return StartCoroutine(GenericFunctions.instance.LerpTransform(renderCam.transform, targetPostion, panningDuration));

        GameManager.instance.isTransitioning = false;
        pannedToZone = true;
    }

    void InitiateRaycast()
    {
        if (!transitioned) return;
        if (GameManager.instance.isTransitioning) return;
        if(GameManager.instance.inDialogue) return;

        Vector2 localPoint;
        Vector2 screenPosition = Input.mousePosition;

        RectTransform rectTransform = rawImage.rectTransform;
        Camera eventCamera = null; // If Canvas is Overlay; otherwise use your UI camera

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPosition, eventCamera, out localPoint))
        {
            if (rectTransform.rect.Contains(localPoint))
            {
                
                if(eventCamera != null) Debug.Log("Using: " + eventCamera.name);
                Vector2 normalized = Rect.PointToNormalized(rectTransform.rect, localPoint);
                float texX = normalized.x * renderTexture.width;
                float texY = normalized.y * renderTexture.height;
                Vector2 textureCoord = new Vector2(texX, texY);

                RaycastHit cameraPanHit = RaycastTargetLayer(textureCoord, 100f);
                HandleRaycast(cameraPanHit);
            }
        }
    }

    void HandleRaycast(RaycastHit _hit)
    {
        if(GameManager.instance.isTransitioning) return;

        if(!GameManager.instance.inBoardView)
        {
            if(Input.GetMouseButtonDown(0))
            {
                if(CheckColliderLayerMask(_hit.collider, capturableLayer)) 
                {
                    if(inPhotoMode) TakePicture(_hit);
                }
                if(CheckColliderLayerMask(_hit.collider, inspectionAreaLayer)) 
                {
                    if(_hit.collider != null && !pannedToZone && !GameManager.instance.inBoardView) StartCoroutine(PanToZone(_hit));
                }
                if(CheckColliderLayerMask(_hit.collider, doorLayer)) 
                {
                    string connectingRooms = _hit.collider.GetComponent<Door>().connectingRooms;
                    StartCoroutine(GameManager.instance.TransitionLevel(connectingRooms));
                    pannedToZone = false;
                }
            }

            if(CheckColliderLayerMask(_hit.collider, characterLayer)) 
            {
                if(_hit.collider.gameObject != lastHoveringChar)
                {
                    if(hoveringChar != null)
                    {
                        //hoveringChar.meshRenderer.material = hoveringChar.defaultCharacterMaterial;
                        hoveringChar.charImg.sprite = hoveringChar.defaultSprite;
                        hoveringChar = null;
                        lastHoveringChar = null;
                    }

                    lastHoveringChar = _hit.collider.gameObject;
                    Debug.Log("New Hovering Char: " + lastHoveringChar.name);
                    hoveringChar = lastHoveringChar.GetComponent<Character>();
                    Debug.Log("New Hovering Char Script: " + hoveringChar.name);
                    //hoveringChar.meshRenderer.material = hoveringChar.hoverCharacterMaterial;
                    hoveringChar.charImg.sprite = hoveringChar.hoverSprite;
                }
            }
            else
            {
                if(hoveringChar != null)
                {
                    if(dialoguingChar != hoveringChar) 
                    {
                        //hoveringChar.meshRenderer.material = hoveringChar.defaultCharacterMaterial;
                        hoveringChar.charImg.sprite = hoveringChar.defaultSprite;
                    }
                    hoveringChar = null;
                    lastHoveringChar = null;
                }
            }
        }
        else
        {
            if(hoveringChar != null)
            {
                hoveringChar.charImg.sprite = hoveringChar.defaultSprite;
                hoveringChar = null;
                lastHoveringChar = null;
            }

            if(_hit.collider == null) return;
            Vector3 hitPoint = _hit.point;

            if(placingPin) 
            {
                Debug.Log("Placing pin at " + hitPoint);
                ropeToBePlaced.Recalculate();
                Vector3 placePosition = hitPoint;
                placePosition.z = pinToBePlaced.transform.position.z;
                pinToBePlaced.transform.position = placePosition;
                if(CheckColliderLayerMask(_hit.collider, pinLayer) && Input.GetMouseButtonDown(0) && _hit.collider.gameObject != originPin) 
                {
                    PlacePin();
                }
                else if(Input.GetKeyDown(KeyCode.Escape))
                {
                    StopPlacingPin();
                }
                return;
            }


            if(CheckColliderLayerMask(_hit.collider, polaroidHoverLayer)) 
            {
                if(lastPolaroid == null) lastPolaroid = _hit.collider.gameObject.GetComponent<Polaroid>();
                if(lastPolaroid != null) lastPolaroid.HandleShowingDescription(true);
            }
            else if(!CheckColliderLayerMask(_hit.collider, wordLayer)) 
            {
                if(lastPolaroid != null)
                {  
                    lastPolaroid.HandleShowingDescription(false);
                    lastPolaroid = null;
                }
            }

            if(CheckColliderLayerMask(_hit.collider, wordLayer) && Input.GetMouseButtonDown(0)) 
            {
                if(lastPolaroid != null)
                {
                    lastPolaroid.SetWord(_hit.collider.gameObject.name);
                }
            }

            if(CheckColliderLayerMask(_hit.collider, pinLayer) && Input.GetMouseButtonDown(0)) 
            {
                //Create pin and thread
                Transform pinLocation = _hit.collider.transform;
                originPin = _hit.collider.gameObject;
                placingPin = true;
                GameManager.instance.placingPin = true;
                otherPin = Instantiate(pin, pinLocation);
                pinToBePlaced = Instantiate(pin, pinLocation);
                ropeToBePlaced = Instantiate(thread, pinLocation);

                ropeToBePlaced.SetStartPoint(originPin.transform);
                ropeToBePlaced.SetEndPoint(pinToBePlaced.transform);

                //ropeToBePlaced.Recalculate();
                //ropeToBePlaced.CustomStart();
            }
        }
    }

    void PlacePin()
    {
        placingPin = false;
        GameManager.instance.placingPin = false;
    }

    void StopPlacingPin()
    {
        Destroy(otherPin);
        Destroy(pinToBePlaced);
        Destroy(ropeToBePlaced.gameObject);
        placingPin = false;
        GameManager.instance.placingPin = false;
    }

    void SetupCamera()
    {
        //renderCam.orthographic = true;
        //renderCam.orthographicSize = orthoSize;
        //renderCam.transform.position = new Vector3(0, 1.6f, orthoDistance);
        //renderCam.transform.rotation = Quaternion.Euler(0, 0, 0);
        renderTexture = renderCam.targetTexture;
        originalFov = renderCam.fieldOfView;
        originalFlashAlpha = cameraFlashImg.color.a;
    }

    void FirstClick()
    {
        if (Input.GetMouseButtonDown(0) && !isTransitioning)
        {
            StartCoroutine(SwitchProjection(!isOrthographic));
            StartCoroutine(GenericFunctions.instance.FlickerLight(overheadLight));
        }
    }

    IEnumerator SwitchProjection(bool toOrthographic)
    {
        isTransitioning = true;

        float time = 0f;

        float startFOV = renderCam.fieldOfView;
        float startSize = renderCam.orthographicSize;

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

    RaycastHit RaycastTargetLayer(Vector2 direction, float distance)
    {
        // Cast a ray from the camera's render texture
        Ray ray = renderCam.ScreenPointToRay(direction);
        RaycastHit hitTarget = new();

        // Visualize the ray in Scene view
        Debug.DrawRay(ray.origin, ray.direction * distance, Color.cyan, 0.5f);

        if (Physics.Raycast(ray, out RaycastHit hit, distance))
        {
            hitTarget = hit;
        }

        return hitTarget;
    }

    bool CheckColliderLayerMask(Collider col, LayerMask layerMask)
    {
        if(col == null) return false;

        if (((1 << col.gameObject.layer) & layerMask) != 0)
        {
            //Debug.Log($"Raycast: {col.gameObject.name} is on {layerMask}");
            return true;
        }

        //Debug.Log($"Raycast: {col.gameObject.name} is not on {layerMask}");
        return false;
    }
}