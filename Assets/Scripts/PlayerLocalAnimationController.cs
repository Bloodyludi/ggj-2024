using System;
using System.Collections;
using UnityEngine;

public class PlayerLocalAnimationController : MonoBehaviour
{
    [SerializeField] private AnimationCurve moveUpCurve;
    [SerializeField] private AnimationCurve moveDownCurve;
    [SerializeField] private AnimationCurve jumpCurve;

    public IEnumerator Jump(float duration)
    {
        Vector2 ogPos = this.transform.localPosition;

        yield return PacedForLoop(duration, lapsedPercent =>
        {
            var transformLocalPosition = ogPos + Vector2.up * jumpCurve.Evaluate(lapsedPercent);
            this.transform.localPosition = transformLocalPosition;
        });
        var transformLocalPosition = ogPos + Vector2.up * jumpCurve.Evaluate(1);
        this.transform.localPosition = transformLocalPosition;
    }


    public IEnumerator MoveDown(float duration)
    {
        Vector2 ogPos = this.transform.localPosition;

        yield return PacedForLoop(duration, lapsedPercent =>
        {
            var transformLocalPosition = ogPos + Vector2.up * moveDownCurve.Evaluate(lapsedPercent);
            this.transform.localPosition = transformLocalPosition;
        });
        var transformLocalPosition = ogPos + Vector2.up * moveDownCurve.Evaluate(1);
        this.transform.localPosition = transformLocalPosition;
    }

    public IEnumerator MoveUp(float duration)
    {
        Vector2 ogPos = this.transform.localPosition;

        yield return PacedForLoop(duration, lapsedPercent =>
        {
            var transformLocalPosition = ogPos + Vector2.up * moveUpCurve.Evaluate(lapsedPercent);
            this.transform.localPosition = transformLocalPosition;
        });

        var transformLocalPosition = ogPos + Vector2.up * moveUpCurve.Evaluate(1);
        this.transform.localPosition = transformLocalPosition;
    }

    private IEnumerator PacedForLoop(float duration, Action<float> onLapsedPercent)
    {
        for (float time = 0; time <= duration; time += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(time / duration);
            onLapsedPercent.Invoke(lapsedPercent);
            yield return new WaitForEndOfFrame();
        }
    }
}