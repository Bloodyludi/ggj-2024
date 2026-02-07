using System;
using System.Collections;
using UnityEngine;

public static class CoroutineUtils
{
    public static IEnumerator PacedForLoop(float duration, Action<float> onLapsedPercent)
    {
        for (float time = 0; time <= duration; time += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(time / duration);
            onLapsedPercent.Invoke(lapsedPercent);
            yield return null;
        }
    }
}
