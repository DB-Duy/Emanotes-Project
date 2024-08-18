using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectsManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _greatFx, _perfectFx;
    [SerializeField]
    private ParticleSystem _greatParticles, _perfectParticles;
    private Tween perfect, great;


    private void Start()
    {
        great = DOTween.Sequence()
            .AppendInterval(0.25f)
            .AppendCallback(() => _greatFx.gameObject.SetActive(false))
        .SetRecyclable(true).SetAutoKill(false);
        perfect = DOTween.Sequence()
            .AppendInterval(0.25f)
            .AppendCallback(() => _perfectFx.gameObject.SetActive(false))
        .SetRecyclable(true).SetAutoKill(false);
    }
    public void PlayEffectPerfect()
    {
        _greatFx.SetActive(true);
        _greatParticles.Play();
        great.Restart();
    }
    public void PlayEffectGreat()
    {
        _perfectFx.SetActive(true);
        _perfectParticles.Play();
        perfect.Restart();
    }
}
