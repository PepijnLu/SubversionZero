using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GenericFunctions : MonoBehaviour
{
    public static GenericFunctions instance;
    void Awake()
    {
        instance = this;   
    }

    public IEnumerator FadeImage(Image _image, float _duration, float _target)
    {
        float currentValue = _image.color.a;
        float _elapsedTime = 0;
        Color color = _image.color;

        while (_elapsedTime <= _duration)
        {
            currentValue = Mathf.Lerp(currentValue, _target, _elapsedTime / _duration);
            color.a = currentValue;
            Debug.Log($"Changed {_image.name} alpha to {_image.color.a}");
            _image.color = color;
            _elapsedTime += Time.deltaTime;
            yield return null;
        }

        color.a = _target;
        _image.color = color;
    }

    public IEnumerator LerpTransform(Transform _transform, Vector3 _targetPos, float _duration)
    {
        Vector3 startPos = _transform.position;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            _transform.position = Vector3.Lerp(startPos, _targetPos, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _transform.position = _targetPos;
    }

    public IEnumerator LerpRotation(Transform _transform, Quaternion _targetRot, float _duration)
    {
        Quaternion startRot = _transform.rotation;
        float elapsed = 0f;

        while (elapsed < _duration)
        {
            _transform.rotation = Quaternion.Slerp(startRot, _targetRot, elapsed / _duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        _transform.rotation = _targetRot;
    }

    public IEnumerator LerpFov(float startValue, float targetValue, float duration, Camera _camera)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float currentValue = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            _camera.fieldOfView = currentValue;
            elapsed += Time.deltaTime;
            yield return null;
        }

        _camera.fieldOfView = targetValue;
    }
}
