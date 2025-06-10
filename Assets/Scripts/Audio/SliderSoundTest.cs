using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

public class SliderSoundTest : MonoBehaviour
{
    public Slider uiSlider; // Assign in Inspector
    public EventReference fmodEvent; // Drag FMOD event here in Inspector

    private void Start()
    {
        if (uiSlider != null)
        {
            uiSlider.onValueChanged.AddListener(PlayUISound);
        }
    }

    void PlayUISound(float value)
    {
        if (fmodEvent.IsNull) return;
        RuntimeManager.PlayOneShot(fmodEvent);
    }
}
