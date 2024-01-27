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
            StunPlayer();
        }
    }

    public void StunPlayer()
    {
        if (playerState.CurrentAction == PlayerAction.Stunned) return;
        recoverFromStunBeatNumber = beatManager.BeatCounter + beatStunDuration;
        
        beatManager.OnBeatUpdate += ResolvePlayerStun;
    }

    private void ResolvePlayerStun()
    {
        if (beatManager.BeatCounter > recoverFromStunBeatNumber)
        {
            beatManager.OnBeatUpdate -= ResolvePlayerStun;
            playerState.CurrentAction = PlayerAction.None;
            return;
        }
        Debug.Log($"stunned for{recoverFromStunBeatNumber-beatManager.BeatCounter} ");
        playerState.CurrentAction = PlayerAction.Stunned;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);
        RestartRoutine(Pushback(moveDir *-1));
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