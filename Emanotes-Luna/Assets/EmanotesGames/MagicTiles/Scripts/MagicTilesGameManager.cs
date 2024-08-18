using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MagicTilesGameManager : MonoBehaviour
{
    public static MagicTilesGameManager instance;
    [HideInInspector]
    public bool LostGame = false;
    bool hasStarted = false;
    [SerializeField]
    private Transform _leftLine, _rightLine;
    [SerializeField]
    private MagicTile _tileHold, _tileSingle, _tileHhold51, _tileHold92, _tileHold69;
    private float[] touchTimesArr = new float[]
    {
        0.51923f,0.46153802f,0.46153802f,0.46153796f,0.92307603f,0.46153808f,0.23076892f,0.23076892f,0.46153808f,0.46153784f,0.23076916f,0.23076916f,0.46153784f,0.46153784f,0.23076916f,0.23076916f,0.46153784f,0.46153784f,0.4615383f,0.4615383f,0.2307682f,0.23076916f,0.4615383f,0.4615383f,0.2307682f,0.23076916f,0.4615383f,0.23076916f,0.23076916f,0.46153736f,0.6923075f,0.23076916f,0.2307682f,0.23076916f,0.23076916f,0.23076916f,0.22596169f,0.23557663f,0.46153736f,0.23076916f,0.23076916f,0.4615383f,0.46153736f,0.23076916f,0.2307682f,0.4615383f,0.4615383f,0.23077011f,0.2307682f,0.4615383f,0.4615383f,0.4615364f
    };
    private WaitForSeconds[] _waitTimes;
    private int noteIdx;

    public bool IsTouching = false;
    private float leftCoordinate, rightCoordinate, range;
    [SerializeField, HideInInspector]
    private Camera _camera;
    private MagicTile _currentHoldTile, _tileToTap, _nextTile;
    [SerializeField]
    private ProgressBarManager _progressBarManager;
    private int combo = 0;
    [SerializeField]
    private TMP_Text _comboText;
    [SerializeField]
    private Transform[] _spawnPos;
    [SerializeField]
    private EffectsManager FXManager;
    private void OnValidate()
    {
        _camera = Camera.main;
    }
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        _waitTimes = new WaitForSeconds[touchTimesArr.Length];
        for (int i = 0; i < touchTimesArr.Length; i++)
        {
            _waitTimes[i] = new WaitForSeconds(touchTimesArr[i]);
        }
    }
    private Vector3 handPos;
    private void Start()
    {
        OrientationManager.instance.OnOrientationChanged += RecalculateTouchRanges;
        RecalculateTouchRanges();
        InitializeTilePool();
        _progressBarManager.InitProgressbar(touchTimesArr.Length);
        handPos = _tutorialHand.transform.position;
        _tutorialTween = DOTween.Sequence()
                            .Append(_tutorialHand.transform.DOLocalMove(new Vector3(0.226f, -0.219f, 0), 0.4f))
                            .Join(_tutorialHand.transform.DOScale(0.25f, 0.4f))
                            .SetAutoKill(false)
                            .SetRecyclable(true)
                            .SetLoops(-1, LoopType.Yoyo);
    }

    public void RecalculateTouchRanges()
    {
        leftCoordinate = _leftLine.position.x;
        rightCoordinate = _rightLine.position.x;
        range = (rightCoordinate - leftCoordinate) * 0.25f;
    }
    public int GetSectionTouched(float x)
    {
        return Mathf.FloorToInt((x - leftCoordinate) / range);
    }
    public void OnTouchDown(BaseEventData data)
    {
        if (LostGame)
        {
            Luna.Unity.Playable.InstallFullGame();
            return;
        }
        IsTouching = true;
        HandleRaycast(_camera.ScreenPointToRay((data as PointerEventData).position));
    }

    public void OnTouchUp(BaseEventData data)
    {
        IsTouching = false;
        if (_currentHoldTile != null)
        {
            _currentHoldTile.CancelSlider();
            _currentHoldTile = null;
        }
    }
    public void StartGame()
    {
        hasStarted = true;
        DisableOnStart();
        StartCoroutine(SpawnTiles());
    }
    [SerializeField]
    private GameObject[] _disableOnStart;
    private void DisableOnStart()
    {
        for (int i = 0; i < _disableOnStart.Length; i++)
        {
            _disableOnStart[i].SetActive(false);
        }
    }

    private RaycastHit2D[] results = new RaycastHit2D[1];
    public LayerMask _hitMask;
    private void HandleRaycast(Ray ray)
    {
        if (Physics2D.RaycastNonAlloc(ray.origin, ray.direction, results, 50, _hitMask) > 0)
        {
            MagicTile tile = results[0].transform.parent.GetComponent<MagicTile>();
            if (tile == null)
            {
                LostGameMissTile(results[0].point);
                return;
            }
            if (!hasStarted)
            {
                StartGame();
                _tutorialHand.DOFade(0, 0.2f).OnComplete(() =>
                {
                    _tutorialTween.Pause();
                    _tutorialHand.gameObject.SetActive(false);
                });

                Invoke(nameof(StartMusic), 0.55f);
            }
            else if (!ReferenceEquals(GetValidTile(), tile))
            {
                LostGameInvalidTile(GetValidTile());
                return;
            }
            else
            {
                combo++;
                _comboText.SetText(combo.ToString());
                _progressBarManager.TileTapped();
                tile.OnTap(results[0].point.y);
                if (tile.TileType == TileType.Hold)
                {
                    _currentHoldTile = tile;
                }
                else
                {
                    _currentHoldTile = null;
                }

                Collider2D col = results[0].collider;
                if (col.CompareTag("GreatCol"))
                {
                    FXManager.PlayEffectGreat();
                }
                else if (col.CompareTag("PerfectCol"))
                {
                    FXManager.PlayEffectPerfect();
                }
            }
        }
    }

    [SerializeField]
    private AudioSource _music;
    private void StartMusic()
    {
        _music.Play();
    }
    private MagicTile[] singleTiles = new MagicTile[10];
    private MagicTile[] holdTiles = new MagicTile[10];
    private MagicTile[] activeTiles = new MagicTile[10];
    [SerializeField]
    private float TileSpeed;
    private int randomIndex;
    private int lastIndex = -1;
    public bool autoWin = false;
    private MagicTile GetValidTile()
    {
        MagicTile lowest = null;
        float lowestY = 100;
        int i = 0;
        MagicTile currentTile;
        for (i = 0; i < 10; i++)
        {
            currentTile = singleTiles[i];
            if (!currentTile.gameObject.activeInHierarchy || currentTile.IsTapped) { continue; }
            if (currentTile.transform.position.y < lowestY)
            {
                lowestY = currentTile.transform.position.y;
                lowest = currentTile;
            }
        }
        for (i = 0; i < 10; i++)
        {
            currentTile = holdTiles[i];
            if (!currentTile.gameObject.activeInHierarchy || currentTile.IsTapped) { continue; }
            if (currentTile.transform.position.y < lowestY)
            {
                lowestY = currentTile.transform.position.y;
                lowest = currentTile;
            }
        }
        if (_tileHhold51.gameObject.activeInHierarchy && !_tileHhold51.IsTapped && _tileHhold51.transform.position.y < lowestY)
        {
            lowest = _tileHhold51;
            lowestY = _tileHhold51.transform.position.y;
        }
        if (_tileHold69.gameObject.activeInHierarchy && !_tileHold69.IsTapped && _tileHold69.transform.position.y < lowestY)
        {
            lowest = _tileHold69;
            lowestY = _tileHold69.transform.position.y;
        }
        if (_tileHold92.gameObject.activeInHierarchy && !_tileHold92.IsTapped && _tileHold92.transform.position.y < lowestY)
        {
            lowest = _tileHold92;
            lowestY = _tileHold92.transform.position.y;
        }
        return lowest;
    }

    private void InitializeTilePool()
    {
        for (int i = 0; i < singleTiles.Length; i++)
        {
            GameObject tile = Instantiate(_tileSingle.gameObject);
            tile.SetActive(false);
            singleTiles[i] = tile.GetComponent<MagicTile>();
        }
        for (int i = 0; i < holdTiles.Length; i++)
        {
            GameObject tile = Instantiate(_tileHold.gameObject);
            tile.SetActive(false);
            holdTiles[i] = tile.GetComponent<MagicTile>();
        }
    }
    IEnumerator SpawnTiles()
    {
        for (int i = 0; i < touchTimesArr.Length; i++)
        {
            if (LostGame)
            {
                yield break;
            }
            SpawnTile(touchTimesArr[i]);
            yield return _waitTimes[i];
        }
        Invoke(nameof(LoseGame), 2f);
    }

    void SpawnTile(float length)
    {
        MagicTile tile = GetAvailableTile(length);

        // Randomly pick one of the 4 spawn positions
        randomIndex = UnityEngine.Random.Range(0, _spawnPos.Length);
        while (randomIndex == lastIndex)
        {
            randomIndex = UnityEngine.Random.Range(0, _spawnPos.Length);
        }
        lastIndex = randomIndex;
        Transform spawnPoint = _spawnPos[randomIndex];

        tile.transform.position = spawnPoint.position;
        tile.gameObject.SetActive(true);

        if (length == 0.92307603f || length == 0.6923075f)
        {
            StartCoroutine(MoveTile(tile, -16f));
        }
        else
        {
            StartCoroutine(MoveTile(tile, -8f));
        }
    }

    private MagicTile GetAvailableTile(float length)
    {
        if (length == 0.51923f)
        {
            return _tileHhold51;
        }
        if (length == 0.92307603f)
        {
            return _tileHold92;
        }
        if (length == 0.6923075f)
        {
            return _tileHold69;
        }

        if (length < 0.35f)
        {
            for (int i = 0; i < singleTiles.Length; i++)
            {
                if (!singleTiles[i].gameObject.activeInHierarchy)
                {
                    return singleTiles[i];
                }
            }
        }
        else if (length < 0.5f)
        {
            for (int i = 0; i < holdTiles.Length; i++)
            {
                if (!holdTiles[i].gameObject.activeInHierarchy)
                {
                    return holdTiles[i];
                }
            }
        }
        return null;
    }

    IEnumerator MoveTile(MagicTile tile, float limit)
    {
        Transform tileTransform = tile.transform;

        while (tileTransform.position.y > limit && !LostGame)
        {
            tileTransform.Translate(Vector3.down * TileSpeed * Time.deltaTime);
            if (tileTransform.position.y < -4.5f && !tile.IsTapped && !autoWin)
            {
                LostGameTilePassed(tile);
                yield break;
            }
            yield return null;
        }
        if (!LostGame)
        {
            tile.gameObject.SetActive(false);
        }
    }
    public void LostGameTilePassed(MagicTile tile)
    {
        LoseGame();
        tile.PlayRedAnim();
    }
    public void LostGameInvalidTile(MagicTile tile)
    {
        LoseGame();
        tile.PlayRedAnim();
    }
    public void LostGameMissTile(Vector3 tapPos)
    {
        LoseGame();
        int idx = 0;
        float closest = 100;
        for (int i = 0; i < _spawnPos.Length; i++)
        {
            float dist = Mathf.Abs(tapPos.x - _spawnPos[i].position.x);
            if (dist < closest)
            {
                idx = i;
                closest = dist;
            }
        }
        Vector3 pos = _spawnPos[idx].transform.position;
        MagicTile errorTile = Instantiate(GetValidTile());
        errorTile.transform.position = new Vector3(pos.x, errorTile.transform.position.y, pos.z);
        errorTile.gameObject.SetActive(true);
        errorTile.PlayRedAnim();
    }

    [SerializeField]
    private SpriteRenderer _endmask, _tutorialHand, _logo, _download;
    private Tween _tutorialTween;
    public void LoseGame()
    {
        CancelInvoke(nameof(StartMusic));
        _endmask.gameObject.SetActive(true);
        _endmask.DOFade(0.7f, 1f);
        _music.Pause();
        LostGame = true;
        Luna.Unity.LifeCycle.GameEnded();
        _tutorialHand.transform.position = handPos;
        _logo.gameObject.SetActive(true);
        _download.gameObject.SetActive(true);
        _tutorialHand.gameObject.SetActive(true);
        _tutorialHand.DOFade(1, 0.1f).SetDelay(1f).OnComplete(() => _tutorialTween.Restart());
        _logo.gameObject.SetActive(true);

        _logo.DOFade(1f, 0.5f).SetDelay(0.5f);
        _download.DOFade(1f, 0.5f).SetDelay(0.5f);
    }
}
