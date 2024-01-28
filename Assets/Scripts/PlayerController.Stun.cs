using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public partial class PlayerController
{
    [Header("Stun")] [SerializeField] private int beatStunDuration = 1;

    private int recoverFromStunBeatNumber;

    public void StunPlayer(Vector2 direction)
    {
        transform.localScale = Vector2.one;
        if (playerState.CurrentStateEnum == PlayerStateEnum.Stunned) return;
        recoverFromStunBeatNumber = beatManager.BeatCounter + beatStunDuration;
        moveDir = direction;
    }

    public void SetPlayerFighting()
    {
        playerState.CurrentStateEnum = PlayerStateEnum.Brawl;
        moveDir = Vector2.zero;
        this.transform.localScale = Vector2.zero;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);
        beatManager.OnPostBeat += ResolvePlayerStun;
    }

    private void ResolvePlayerStun()
    {
        if (beatManager.BeatCounter > recoverFromStunBeatNumber)
        {
            beatManager.OnBeat -= ResolvePlayerStun;
            playerState.CurrentStateEnum = PlayerStateEnum.None;
            moveDir = Vector2.zero;
            return;
        }

        playerState.CurrentStateEnum = PlayerStateEnum.Stunned;
        RestartRoutine(Move( moveDir));
    }
}