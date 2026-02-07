using System;
using UnityEngine;
using System.Collections;
using Random = UnityEngine.Random;

public class ShakeOnBeat : MonoBehaviour
{
    private BeatManager beatManager;
    [SerializeField] private float magnitude = 2;
    [SerializeField, Range(0, 0.5f)] private float percentOfWindowSeconds = 0.3f;
    private Vector3 originalPos;

    private void Awake()
    {
        // Fix: Use FindFirstObjectByType (New API)
        beatManager = FindFirstObjectByType<BeatManager>();
    }

    private void OnDisable()
    {
        if(beatManager) beatManager.OnBeat -= OnBeatManagerOnOnBeat;
    }

    private void OnEnable()
    {
        if(beatManager) beatManager.OnBeat += OnBeatManagerOnOnBeat;
    }

    void OnBeatManagerOnOnBeat()
    {
        if(beatManager)
            StartCoroutine(Shake(beatManager.MoveWindowSeconds * percentOfWindowSeconds, magnitude));
    }


    public IEnumerator Shake(double duration, float magnitude)
    {
        Vector3 originalPos = transform.localPosition;

        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(x, y, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}