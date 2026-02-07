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

            var moveWindow = (float)beatManager.MoveWindowSeconds;

            bool hitBeat = (lapsedTimeSinceBeat <= moveWindow) || (timeUntilNextBeat <= moveWindow);

            if (hitBeat)
            {
                if (lapsedTimeSinceBeat <= moveWindow)
                    blockedUntil = beatManager.LastBeatTime + moveWindow;
                else
                    blockedUntil = beatManager.NextBeatTime + moveWindow;

                MoveOnBeat();
            }
            else
            {
                playerState.ComboCounter = 0;
                blockedUntil = time + 0.1f;
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

    private IEnumerator Move(Vector2 direction, bool AnimatesSprites = false)
    {
        float movementDuration = beatManager.BeatInterval * 0.2f;
        var position = transform.position;
        Vector2 ogPos = position;

        if (AnimatesSprites)
        {
            HandleLocalAnimations(direction, movementDuration);
        }

        yield return CoroutineUtils.PacedForLoop(movementDuration, lapsedPercent =>
        {
            var currentPosition = ogPos + moveDir * lapsedPercent * mapManager.TileSize;
            transform.position = mapManager.GetLoopPosition(currentPosition);
        });

        position = ogPos + direction * mapManager.TileSize;
        position = mapManager.GetLoopPosition(position);
        transform.position = position;
        mapManager.OnPlayerPositionUpdated(this);
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
        // Miss logic handled in AttemptMove
    }
}
