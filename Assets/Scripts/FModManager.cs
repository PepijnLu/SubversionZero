using UnityEngine;
using FMODUnity;

public class FModManager : MonoBehaviour
{
    public static FModManager instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;    
    }

    public void SetParameter(StudioEventEmitter _emitter, string paramName, float value)
    {
        if (_emitter != null && _emitter.EventInstance.isValid())
        {
            _emitter.EventInstance.setParameterByName(paramName, value);
        }
    }
}
