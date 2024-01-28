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
    public event Action OnCustomUpdate;
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private CountdownTimer countdownTimer;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private BeatManager beatManager;
    [SerializeField] private int matchDurationSeconds = 180;
    [SerializeField] private int musicSpeedUpThreshold = 60;

    private float matchTimeElapsed;
    private bool isSpeedUpTriggered1;
    private bool isSpeedUpTriggered2;

    public bool IsGameOver { get; private set; } = false;
    public GameResult gameResult = GameResult.Draw;

    private void Awake()
    {
        Time.timeScale = 1f;

        isSpeedUpTriggered1 = false;
        isSpeedUpTriggered2 = false;
        soundManager.Init();
        beatManager.Init();
        StartCoroutine(StartMatch());

    }

    private IEnumerator StartMatch()
    {
        while (true)
        {
            var timeLeft = matchDurationSeconds - matchTimeElapsed;
            countdownTimer.UpdateTimeLeft(timeLeft);

            matchTimeElapsed += Time.deltaTime;

            if (matchTimeElapsed > 5f &&
                playerManager.player1State.CurrentStateEnum == PlayerStateEnum.Dead
                && playerManager.player2State.CurrentStateEnum == PlayerStateEnum.Dead)
            {
                Time.timeScale = Mathf.Lerp(Time.timeScale, 6f, Time.deltaTime * 0.2f);
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
        PauseGame(true);
        gameOverScreen.Show(gameResult);
        IsGameOver = true;
    }

    private void EvaluateGameOver()
    {
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