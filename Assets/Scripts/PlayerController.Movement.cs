using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController
{
    [SerializeField] private float movementDistance = 1f;
    
    [Header("Movement Animations")] 
    [SerializeField] private AnimationCurve horizontalJumpAnimationCurve;

    [SerializeField] private AnimationCurve verticalUpwardsJumpAnimationCurve;
    [SerializeField] private AnimationCurve verticalDownwardsJumpAnimationCurve;

    private float moveRecordTime = 0;
    private IEnumerator currentMoveRoutine;
    private void OnMoveRegistered(InputAction.CallbackContext context)
    {
        if (Time.time <= blockedTime)
        {
            return;
        }

        if (playerState.CanWalk && context.phase == InputActionPhase.Started)
        {
            moveRecordTime = Time.time;
            moveDir = context.ReadValue<Vector2>();

            var lapsedTimeSinceBeat = moveRecordTime - beatManager.LastBeatTime;
            var timeUntilNextBeat = beatManager.NextBeatTime - moveRecordTime;
            var moveWindowSeconds = beatManager.moveWindowTimePercent * beatManager.SecondsPerBeat / 100;

            if (lapsedTimeSinceBeat <= moveWindowSeconds)
            {
                Debug.Log($"Moving after beat: {lapsedTimeSinceBeat}");
                blockedTime = beatManager.LastBeatTime + moveWindowSeconds;
                MoveOnBeat();
            }
            else if (timeUntilNextBeat <= moveWindowSeconds)
            {
                Debug.Log($"Moving before beat: {timeUntilNextBeat}");
                blockedTime = beatManager.NextBeatTime + moveWindowSeconds;
                MoveOnBeat();
            }
        }
    }

    private void MoveOnBeat()
    {
        RestartJumpRoutine();

        playerState.WalkDirection = moveDir;
        if (Mathf.Abs(moveDir.x) > 0)
        {
            playerState.PlayerOrientation = (int)Mathf.Sign(moveDir.x);
        }
    }

    private void RestartJumpRoutine()
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }

        currentMoveRoutine = Move(moveDir);
        StartCoroutine(currentMoveRoutine);
    }

    private IEnumerator Move(Vector2 direction)
    {
        float tempo = beatManager.SecondsPerBeat * 0.2f;
        var ogPos = transform.position;
        Vector2 currentPosition = ogPos;

        for (float t = 0; t <= tempo; t += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(t / tempo);

            if (direction.x != 0)
            {
                currentPosition = MoveHorizontally(direction, lapsedPercent, ogPos);
            }
            else
            {
                currentPosition = MoveVertically(direction, lapsedPercent, currentPosition, ogPos);
            }

            transform.position = currentPosition;
            yield return new WaitForEndOfFrame();
        }

        transform.position = ogPos + (Vector3)direction * movementDistance;
        currentMoveRoutine = null;
        yield return null;
    }

    private Vector2 MoveVertically(Vector2 direction, float lapsedPercent, Vector2 currentPosition, Vector3 ogPos)
    {
        float y;
        switch (direction.y)
        {
            case > 0:
                y = verticalUpwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                currentPosition.y = ogPos.y + y * movementDistance;
                break;
            case < 0:
                y = verticalDownwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                currentPosition.y = ogPos.y - y * movementDistance;
                break;
        }

        return currentPosition;
    }

    private Vector2 MoveHorizontally(Vector2 direction, float lapsedPercent, Vector3 ogPos)
    {
        float y;
        Vector2 currentPosition;
        y = horizontalJumpAnimationCurve.Evaluate(lapsedPercent);
        currentPosition.x = ogPos.x + lapsedPercent * direction.x * movementDistance;
        currentPosition.y = ogPos.y + y * movementDistance;
        return currentPosition;
    }
}