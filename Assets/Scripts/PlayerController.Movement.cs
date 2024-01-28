using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController
{
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
            var moveWindowSeconds = beatManager.MoveWindowTimePercent * beatManager.SecondsPerBeat / 100;

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
        playerState.ComboCounter++;
        
        RestartRoutine( Move(moveDir));
        UpdateSpriteDirection();
    }

    private void UpdateSpriteDirection()
    {
        playerState.WalkDirection = moveDir;
        if (Mathf.Abs(moveDir.x) > 0)
        {
            playerState.PlayerOrientation = (int)Mathf.Sign(moveDir.x);
        }
    }

    private IEnumerator PacedForLoop( float duration, Action<float> onLapsedPercent )
    {
        for (float time = 0; time <= duration; time += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(time / duration);
            onLapsedPercent.Invoke(lapsedPercent);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Move(Vector2 direction)
    {
        float movementDuration = beatManager.SecondsPerBeat * 0.2f;
        var position = transform.position;
        var ogPos = position;
        Vector2 currentPosition = ogPos;
        yield return PacedForLoop(movementDuration, lapsedPercent =>
        {
            if (direction.x != 0)
            {
                currentPosition = MoveHorizontally(direction, lapsedPercent, ogPos);
            }
            else
            {
                currentPosition = MoveVertically(direction, lapsedPercent, currentPosition, ogPos);
            }

            transform.position = currentPosition;
        });

        position = ogPos + (Vector3)direction * mapManager.TileSize;
        transform.position = position;
        currentMoveRoutine = null;
        mapManager.OnPLayerPositionUpdated(this);
        yield return null;
    }

    private Vector2 MoveVertically(Vector2 direction, float lapsedPercent, Vector2 currentPosition, Vector3 ogPos)
    {
        float y;
        switch (direction.y)
        {
            case > 0:
                y = verticalUpwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                currentPosition.y = ogPos.y + y * mapManager.TileSize;
                break;
            case < 0:
                y = verticalDownwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                currentPosition.y = ogPos.y - y * mapManager.TileSize;
                break;
        }

        return currentPosition;
    }

    private Vector2 MoveHorizontally(Vector2 direction, float lapsedPercent, Vector3 ogPos)
    {
        float y;
        Vector2 currentPosition;
        y = horizontalJumpAnimationCurve.Evaluate(lapsedPercent);
        currentPosition.x = ogPos.x + lapsedPercent * direction.x * mapManager.TileSize;
        currentPosition.y = ogPos.y + y * mapManager.TileSize;
        return currentPosition;
    }

    private void CheckPlayedMoved()
    {
        if (moveRecordTime < beatManager.LastBeatTime - beatManager.MoveWindowSeconds)
        {
            playerState.ComboCounter = 0;
        }
    }
}