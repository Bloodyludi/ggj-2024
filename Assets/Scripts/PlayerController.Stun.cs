using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public partial class PlayerController
{
    [Header("Stun")] [SerializeField] private int beatStunDuration = 1;

    private int recoverFromStunBeatNumber;

    public void StunPlayer(Vector2 direction)
    {
        transform.localScale = Vector2.one;

        if (playerState.CurrentStateEnum == PlayerStateEnum.Stun) return;
        playerState.CurrentStateEnum = PlayerStateEnum.Stun;
        recoverFromStunBeatNumber = beatManager.BeatCounter + beatStunDuration;
        moveDir = direction;

        beatManager.OnBeat -= ResolvePlayerStun;
        beatManager.OnBeat += ResolvePlayerStun;
    }

    public void SetPlayerFighting(Vector3 cloudPosition)
    {
        Debug.Log($"[Stun] {this.name} entering brawl");
        playerState.CurrentStateEnum = PlayerStateEnum.Brawl;
        moveDir = Vector2.zero;
        this.transform.position = cloudPosition;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);

        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }

    }

    private void ResolvePlayerStun()
    {
        if (playerState.CurrentStateEnum == PlayerStateEnum.Dead)
        {
            return;
        }
        if (beatManager.BeatCounter > recoverFromStunBeatNumber)
        {
            transform.localScale = Vector2.one;

            Debug.Log($"[Stun] {this.name} recovering back to none");

            beatManager.OnBeat -= ResolvePlayerStun;
            playerState.CurrentStateEnum = PlayerStateEnum.None;
            moveDir = Vector2.zero;
            return;
        }

        Debug.Log($"[Stun] {this.name} still stunned");

        RestartRoutine(Move(moveDir));
    }
}
