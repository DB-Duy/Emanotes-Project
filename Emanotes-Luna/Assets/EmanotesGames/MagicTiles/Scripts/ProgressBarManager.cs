using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Rendering.HybridV2;
using UnityEngine;
using UnityEngine.EventSystems;

public class ProgressBarManager : MonoBehaviour
{
    [SerializeField]
    private Transform _worldProgressObject;
    [SerializeField]
    private Transform _leftPoint, _rigthPoint;
    [SerializeField]
    private Camera _camera;
    private Vector3 worldPositionLeft, worldPositionRight;
    public float Progress = 0;
    private int tileCount;
    [SerializeField]
    private SpriteRenderer _progressFill;
    public float _fillMax = 67f;
    float increment;
    [SerializeField]
    private SpriteRenderer[] _spritesLocked, _spritesUnlocked;
    private float currentThreshold = 0.17f;
    private int unlockIdx = 0;
    private void OnValidate()
    {
        _camera = Camera.main;
    }

    public void Update()
    {
        UpdateProgess();
    }

    public void InitProgressbar(int count)
    {
        tileCount = count;
        increment = 1f / tileCount;
        worldPositionLeft = _leftPoint.position;
        worldPositionRight = _rigthPoint.position;
        worldPositionLeft.z = 0;
        worldPositionRight.z = 0;
        UpdateProgess();
    }
    public void UpdateProgess()
    {
        _worldProgressObject.position = Vector3.Lerp(worldPositionLeft, worldPositionRight, Progress);
        _progressFill.size = new Vector2(Mathf.Lerp(0, _fillMax, Progress), 0.5f);
        if (Progress >= 1) { return; }
        if (Progress >= currentThreshold && currentThreshold <= 1)
        {
            currentThreshold = Mathf.Min(currentThreshold + 0.17f, 1);
            SpriteRenderer unlocked = _spritesUnlocked[unlockIdx];
            SpriteRenderer locked = _spritesLocked[unlockIdx];
            unlocked.gameObject.SetActive(true);
            locked.gameObject.SetActive(false);
            DOTween.Sequence()
                .Append(unlocked.transform.DOLocalRotate(Vector3.zero, 0.5f))
                .Join(unlocked.transform.DOScale(1.5f, 0.25f).SetLoops(2, LoopType.Yoyo));
            unlockIdx++;
        }
    }
    public void TileTapped()
    {
        Progress += increment;
        UpdateProgess();
    }
}
