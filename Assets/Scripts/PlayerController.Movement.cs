using System;
using System.Collections;
using UnityEngine;

public partial class PlayerController
{
    private float moveRecordTime = 0;
    private IEnumerator currentMoveRoutine;

    public void AttemptMove(Vector2 direction)
    {
        var time = beatManager.GetCurrentTime();
        
        if (time <= blockedUntil)
        {
            return;
        }

        if (playerState.CanWalk)
        {
            moveRecordTime = time;
            moveDir = direction;

            var lapsedTimeSinceBeat = moveRecordTime - beatManager.LastBeatTime;
            var timeUntilNextBeat = beatManager.NextBeatTime - moveRecordTime;
            
            // Cast to float as BeatManager uses double for precision
            var moveWindow = (float)beatManager.MoveWindowSeconds;

            // Check if we are inside the valid window (either just after the last beat, or just before the next one)
            bool hitBeat = (lapsedTimeSinceBeat <= moveWindow) || (timeUntilNextBeat <= moveWindow);

            if (hitBeat)
            {
                // Calculate lockout based on which beat we hit
                if (lapsedTimeSinceBeat <= moveWindow)
                    blockedUntil = beatManager.LastBeatTime + moveWindow;
                else
                    blockedUntil = beatManager.NextBeatTime + moveWindow;

                MoveOnBeat();
            }
            else
            {
                // --- MISS LOGIC ADDED HERE ---
                // Resetting combo to 0 triggers "PlayerStateEnum.MissedBeat" in PlayerState.cs
                // This will fire the OnStateChanged event and update the Animator.
                playerState.ComboCounter = 0;

                // Optional: Short lockout to prevent spamming the miss animation
                blockedUntil = time + 0.1f; 
                
                Debug.Log($"Missed Beat! Time: {time}");
            }
        }
    }

    private void MoveOnBeat()
    {
        playerState.ComboCounter++;
        RestartRoutine(Move(moveDir, true));
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

    private IEnumerator Move(Vector2 direction, bool AnimatesSprites = false)
    {
        float movementDuration = beatManager.BeatInterval * 0.2f;
        var position = transform.position;
        Vector2 ogPos = position;
        
        if (AnimatesSprites)
        {
            HandleLocalAnimations(direction, movementDuration);
        }

        yield return PacedForLoop(movementDuration, lapsedPercent =>
        {
            transform.position = Vector2.Lerp(ogPos, ogPos + direction * mapManager.TileSize, lapsedPercent);
        });

        transform.position = ogPos + direction * mapManager.TileSize;
        
        mapManager.OnPLayerPositionUpdated(this);
        currentMoveRoutine = null;
    }

    private void HandleLocalAnimations(Vector2 direction, float movementDuration)
    {
        if (direction.x != 0)
        {
            StartCoroutine(playerLocalAnimationController.Jump(movementDuration));
        }
        else if (direction.y > 0)
        {
            StartCoroutine(playerLocalAnimationController.MoveUp(movementDuration));
        }
        else if (direction.y < 0)
        {
            StartCoroutine(playerLocalAnimationController.MoveDown(movementDuration));
        }
    }

    private void CheckPlayerMoved()
    {
        // Optional logic for missed beats
    }
}