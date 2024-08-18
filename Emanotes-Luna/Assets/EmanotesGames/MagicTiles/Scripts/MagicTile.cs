using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Rendering.HybridV2;
using UnityEngine;

public enum TileType
{
    Single = 0,
    Hold = 1
}
public class MagicTile : MonoBehaviour
{
    public TileType TileType = 0;
    [SerializeField, HideInInspector]
    private SpriteRenderer[] childSpriteRenderers;
    public SpriteRenderer TileSlider;
    public SpriteRenderer _mainSprite;
    public bool playingTileSlider;
    public float SliderHeight;
    private float targetHeight;
    private float width = 2.917537f;
    public bool IsTapped = false;
    [SerializeField]
    private SpriteRenderer _red;
    private void OnValidate()
    {
        childSpriteRenderers = GetComponentsInChildren<SpriteRenderer>();
    }
    private void ChangeAlpha(float target)
    {
        int i = 0;
        SpriteRenderer sprite;
        Color c;
        int len = childSpriteRenderers.Length;
        for (i = 0; i < len; i++)
        {
            sprite = childSpriteRenderers[i];
            c = sprite.color;
            c.a = target;
            sprite.color = c;
        }
    }
    public void OnTap(float height)
    {
        IsTapped = true;
        if (TileType == TileType.Single)
        {
            ChangeAlpha(0.5f);
        }
        else
        {
            targetHeight = height;
            PlayTileSlider();
        }
    }

    private void Update()
    {
        if (!playingTileSlider) { return; }

        Vector3 target = transform.InverseTransformPoint(new Vector3(transform.position.x, targetHeight, transform.position.z));
        float height = target.y;
        if (height >= SliderHeight)
        {
            CancelSlider();
        }
        else
        {
            TileSlider.size = new Vector2(width, height);
        }
    }
    public void PlayRedAnim()
    {
        _mainSprite.gameObject.SetActive(false);
        _red.gameObject.SetActive(true);
        Color c = _red.color;
        c.a = 0;
        _red.color = c;
        DOTween.Sequence()
            .Append(_red.DOFade(0.5f, 0.1f))
            .Append(_red.DOFade(0.2f, 0.1f))
            .Append(_red.DOFade(0.3f, 0.1f));
    }
    private void PlayTileSlider()
    {
        playingTileSlider = true;
        Vector3 target = transform.InverseTransformPoint(new Vector3(transform.position.x, targetHeight, transform.position.z));
        TileSlider.size = new Vector2(width, target.y);
    }
    public void CancelSlider()
    {
        ChangeAlpha(0.5f);
        playingTileSlider = false;
    }

    private void OnDisable()
    {
        ChangeAlpha(1);
        if (TileType == TileType.Hold)
        {
            playingTileSlider = false;
            TileSlider.size = new Vector2(width, 0);
        }
        transform.position = new Vector3(0, 6, 0);
        IsTapped = false;
    }
}
