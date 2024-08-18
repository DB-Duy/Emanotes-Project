using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationManager : MonoBehaviour
{
    public static OrientationManager instance;
    public event Action OnOrientationChanged;
    private bool _isLandscape;
    private WaitForEndOfFrame _wait1Frame = new WaitForEndOfFrame();
    [SerializeField]
    private Camera _camera;
    [SerializeField]
    private BoxCollider2D _target;
    public bool IsLandscape
    {
        get { return _isLandscape; }
        set
        {
            if (value != IsLandscape)
            {
                _isLandscape = value;
                StartCoroutine(InvokeAfterFrame());
            }
        }
    }
    private IEnumerator InvokeAfterFrame()
    {
        yield return new WaitForEndOfFrame();
        OnOrientationChanged?.Invoke();

    }
    private void UpdateCam()
    {
        float screenRatio = (float)Screen.width / (float)Screen.height;
        float targetRatio = _target.bounds.size.x / _target.bounds.size.y;
        if (screenRatio >= targetRatio)
        {
            _camera.orthographicSize = _target.bounds.size.y / 2;
        }
        else
        {
            float differenceInSize = targetRatio / screenRatio;
            _camera.orthographicSize = _target.bounds.size.y / 2 * differenceInSize;
        }

    }
    private void OnValidate()
    {
        _camera = Camera.main;
    }
    private void Awake()
    {
        if (instance == null) { instance = this; }
        OnOrientationChanged += UpdateCam;
    }
    private void Start()
    {
        IsLandscape = Screen.width > Screen.height;
        OnOrientationChanged?.Invoke();
    }
    private void LateUpdate()
    {
        IsLandscape = Screen.width > Screen.height;
    }
    private void OnDestroy()
    {
        OnOrientationChanged -= UpdateCam;
    }
}