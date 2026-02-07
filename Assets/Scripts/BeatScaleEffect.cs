using System.Collections;
using UnityEngine;

public class BeatScaleEffect : MonoBehaviour
{
    [SerializeField] private float scaleBoost = 0.4f;

    private BeatManager beatManager;
    private Vector3 baseScale;
    private Coroutine scaleRoutine;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void Start()
    {
        beatManager = Services.Get<BeatManager>();
        if (beatManager != null)
        {
            beatManager.OnPreBeat -= OnPreBeat;
            beatManager.OnPreBeat += OnPreBeat;
            beatManager.OnBeat -= OnBeatHit;
            beatManager.OnBeat += OnBeatHit;
            beatManager.OnPostBeat -= OnPostBeat;
            beatManager.OnPostBeat += OnPostBeat;
        }
    }

    private void OnDisable()
    {
        if (beatManager != null)
        {
            beatManager.OnPreBeat -= OnPreBeat;
            beatManager.OnBeat -= OnBeatHit;
            beatManager.OnPostBeat -= OnPostBeat;
        }
    }

    private void OnPreBeat()
    {
        AnimateTo(baseScale * (1f + scaleBoost), (float)beatManager.MoveWindowSeconds);
    }

    private void OnBeatHit()
    {
        AnimateTo(baseScale, (float)beatManager.MoveWindowSeconds);
    }

    private void OnPostBeat()
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        transform.localScale = baseScale;
        scaleRoutine = null;
    }

    private void AnimateTo(Vector3 target, float duration)
    {
        if (scaleRoutine != null) StopCoroutine(scaleRoutine);
        scaleRoutine = StartCoroutine(ScaleRoutine(target, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 target, float duration)
    {
        Vector3 from = transform.localScale;
        yield return CoroutineUtils.PacedForLoop(duration, t =>
        {
            var smooth = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(from, target, smooth);
        });
        transform.localScale = target;
        scaleRoutine = null;
    }
}
