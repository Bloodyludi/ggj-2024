using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController
{
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
                // Debug.Log($"Moving after beat: {lapsedTimeSinceBeat}");
                blockedTime = beatManager.LastBeatTime + moveWindowSeconds;
                MoveOnBeat();
            }
            else if (timeUntilNextBeat <= moveWindowSeconds)
            {
                // Debug.Log($"Moving before beat: {timeUntilNextBeat}");
                blockedTime = (float)(beatManager.NextBeatTime + moveWindowSeconds);
                MoveOnBeat();
            }
        }
    }

    private void MoveOnBeat()
    {
        playerState.ComboCounter++;

        RestartRoutine(Move(moveDir,true));
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

    private IEnumerator PacedForLoop(float duration, Action<float> onLapsedPercent)
    {
        for (float time = 0; time <= duration; time += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(time / duration);
            onLapsedPercent.Invoke(lapsedPercent);
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Move(Vector2 direction,bool AnimatesSprites =false)
    {
        float movementDuration = beatManager.SecondsPerBeat * 0.2f;
        var position = transform.position;
        Vector2 ogPos = position;
        Vector2 currentPosition = ogPos;

        if (AnimatesSprites)
        {
            HandleLocalAnimations(direction, movementDuration);
        }
        
        yield return PacedForLoop(movementDuration, lapsedPercent =>
        {
            currentPosition = ogPos + moveDir * lapsedPercent * mapManager.TileSize;
            transform.position =  mapManager.GetLoopPosition(currentPosition);
        });

        position = ogPos + (Vector2)direction * mapManager.TileSize;
        position = mapManager.GetLoopPosition(position);
        transform.position = position;
        mapManager.OnPLayerPositionUpdated(this);
        
        currentMoveRoutine = null;
        yield return null;
    }

    private void HandleLocalAnimations(Vector2 direction, float movementDuration)
    {
        if (direction.x != 0)
        {
            StartCoroutine(playerLocalAnimationController.Jump(movementDuration));
        }

        switch (direction.y)
        {
            case > 0:
                StartCoroutine(playerLocalAnimationController.MoveUp(movementDuration));
                break;
            case < 0:
                StartCoroutine(playerLocalAnimationController.MoveDown(movementDuration));
                break;
        }
    }

    private void CheckPlayerMoved()
    {
        if (moveRecordTime < beatManager.LastBeatTime - beatManager.MoveWindowSeconds)
        {
            playerState.ComboCounter = 0;
        }
    }
}