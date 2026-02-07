using System;
using System.Collections;
using UnityEngine;

public enum GameResult
{
    Player1Wins,
    Player2Wins,
    Draw,
    Lose
}

public class GameController : MonoBehaviour
{
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private BeatManager beatManager;
    [SerializeField] private int matchDurationSeconds = 180;

    private float matchTimeElapsed;

    public bool IsGameOver { get; private set; } = false;
    public GameResult gameResult = GameResult.Draw;

    private void Start()
    {
        // Changed from Awake to Start to give other scripts time to initialize
        Time.timeScale = 1f;
        if(soundManager) soundManager.Init();
        StartCoroutine(StartMatch());
    }

    private IEnumerator StartMatch()
    {
        // --- SAFETY WAIT ---
        // Wait until players are spawned by PlayerManager
        yield return new WaitUntil(() => playerManager.player1State != null && playerManager.player2State != null);
        
        if(soundManager) soundManager.PlayMusic();
        if(beatManager) beatManager.ShouldPerformTicks = true;
        
        while (true)
        {
            var timeLeft = matchDurationSeconds - matchTimeElapsed;
            if(countdownTimer) countdownTimer.UpdateTimeLeft(timeLeft);

            matchTimeElapsed += Time.deltaTime;

            // Death Wait Logic
            if (playerManager.player1State != null && playerManager.player2State != null &&
                playerManager.player1State.CurrentStateEnum == PlayerStateEnum.Dead && 
                playerManager.player2State.CurrentStateEnum == PlayerStateEnum.Dead)
            {
                // Pause for dramatic effect
                yield return new WaitForSeconds(1.5f);

                // Snap timer to 0
                if(countdownTimer) countdownTimer.UpdateTimeLeft(0);

                EvaluateGameOver();
                yield break; 
            }

            if (matchTimeElapsed > matchDurationSeconds)
            {
                EvaluateGameOver();
                yield break;
            }

            yield return null;
        }
    }

    public void GameOver()
    {
        if(beatManager) beatManager.ShouldPerformTicks = false;
        PauseGame(true);
        if(gameOverScreen) gameOverScreen.Show(gameResult);
        IsGameOver = true;
    }

    private void EvaluateGameOver()
    {
        // Defensive checks
        if (playerManager.player1State == null || playerManager.player2State == null) return;

        var p1Dead = playerManager.player1State.CurrentStateEnum == PlayerStateEnum.Dead;
        var p2Dead = playerManager.player2State.CurrentStateEnum == PlayerStateEnum.Dead;
        var p1ComboCount = playerManager.player1State.ComboCounter;
        var p2ComboCount = playerManager.player2State.ComboCounter;
        
        if (p1Dead && p2Dead)
        {
            gameResult = GameResult.Lose;
        }
        else if (p1Dead)
        {
            gameResult = GameResult.Player2Wins;
        }
        else if (p2Dead)
        {
            gameResult = GameResult.Player1Wins;
        }
        else if (p1ComboCount > p2ComboCount)
        {
            gameResult = GameResult.Player1Wins;
        }
        else if (p1ComboCount < p2ComboCount)
        {
            gameResult = GameResult.Player2Wins;
        }
        else
        {
            gameResult = GameResult.Draw;
        }
        
        GameOver();
    }

    public void PauseGame(bool pause)
    {
        playerManager.EnablePlayerInput(!pause);
        Time.timeScale = pause ? 0f : 1f;
    }
}