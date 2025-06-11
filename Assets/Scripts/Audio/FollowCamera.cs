using UnityEngine;

[RequireComponent(typeof(FMODUnity.StudioListener))]
public class FollowRenderCam : MonoBehaviour
{
    Transform renderCamTransform;

    void Start()
    {
        renderCamTransform = GameManager.instance.RenderCamTransform;
    }

    void LateUpdate()
    {
        if (renderCamTransform != null)
        {
            transform.position = renderCamTransform.position;
            transform.rotation = renderCamTransform.rotation;
        }
    }
}
