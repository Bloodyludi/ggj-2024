using System.Collections;
using UnityEngine;

public partial class PlayerController
{
    [SerializeField] private float beatStunDuration = 1.0f;

    public void StunPlayer()
    {
        if (playerState.CurrentAction == PlayerAction.Stunned) return;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);
        StartCoroutine(PlayerStunned());
    }

    private IEnumerator PlayerStunned()
    {
        var currentAction = playerState.CurrentAction;
        playerState.CurrentAction = PlayerAction.Stunned;
        yield return new WaitForSeconds(beatStunDuration);
        playerState.CurrentAction = currentAction;
    }
}