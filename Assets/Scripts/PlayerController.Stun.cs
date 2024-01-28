using System.Collections;
using UnityEngine;

public partial class PlayerController
{
    [Header("Stun")] [SerializeField] private int beatStunDuration = 1;

    private int recoverFromStunBeatNumber;

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            StunPlayer(moveDir*-1);
        }
    }

    public void StunPlayer(Vector2 direction)
    {
        if (playerState.CurrentStateEnum == PlayerStateEnum.Stunned) return;
        recoverFromStunBeatNumber = beatManager.BeatCounter + beatStunDuration;
        moveDir = direction;
        beatManager.OnBeatUpdate += ResolvePlayerStun;
    }

    private void ResolvePlayerStun()
    {
        if (beatManager.BeatCounter > recoverFromStunBeatNumber)
        {
            beatManager.OnBeatUpdate -= ResolvePlayerStun;
            playerState.CurrentStateEnum = PlayerStateEnum.None;
            moveDir = Vector2.zero;
            return;
        }

        Debug.Log($"stunned {this.name} for {recoverFromStunBeatNumber - beatManager.BeatCounter} ");
        playerState.CurrentStateEnum = PlayerStateEnum.Stunned;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);
        RestartRoutine(Pushback(moveDir * -1));
    }

    private IEnumerator Pushback(Vector2 direction)
    {
        float movementDuration = beatManager.SecondsPerBeat * 0.2f;
        var ogPos = transform.position;
        Vector2 currentPosition = ogPos;
        yield return PacedForLoop(movementDuration, lapsedPercent =>
        {
            //this.transform.position += (Vector3)direction * movementDistance * lapsedPercent;
        });

        transform.position = ogPos + (Vector3)direction * mapManager.TileSize;
        currentMoveRoutine = null;
        yield return null;
    }
}