using System;
using System.Collections;
using UnityEngine;

public enum GameResult
{
    Player1Wins,
    Player2Wins,
    Draw
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

            if (matchTimeElapsed > matchDurationSeconds)
            {
                GameOver();

                yield break;
            }

            yield return null;
        }
    }

    public void GameOver()
    {
        PauseGame(true);
        gameOverScreen.Show(mapManager.GetGameResult());
        IsGameOver = true;
    }

    public void PauseGame(bool pause)
    {
        playerManager.EnablePlayerInput(!pause);
        Time.timeScale = pause ? 0f : 1f;
    }
}