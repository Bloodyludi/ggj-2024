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

        yield return CoroutineUtils.PacedForLoop(duration, lapsedPercent =>
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

        yield return CoroutineUtils.PacedForLoop(duration, lapsedPercent =>
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

        yield return CoroutineUtils.PacedForLoop(duration, lapsedPercent =>
        {
            var transformLocalPosition = ogPos + Vector2.up * moveUpCurve.Evaluate(lapsedPercent);
            this.transform.localPosition = transformLocalPosition;
        });

        var transformLocalPosition = ogPos + Vector2.up * moveUpCurve.Evaluate(1);
        this.transform.localPosition = transformLocalPosition;
    }
}
