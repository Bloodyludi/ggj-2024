using System.Collections;
using System.Collections.Generic;
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
    private PlayerManager playerManager;
    private MapManager mapManager;
    [SerializeField] private GameOverScreen gameOverScreen;
    [SerializeField] private CountdownTimer countdownTimer;
    private SoundManager soundManager;
    private BeatManager beatManager;
    [SerializeField] private int matchDurationSeconds = 180;

    private float matchTimeElapsed;

    public bool IsGameOver { get; private set; } = false;
    public GameResult gameResult = GameResult.Draw;

    private void Awake()
    {
        Services.Register(this);
    }

    private void Start()
    {
        playerManager = Services.Get<PlayerManager>();
        mapManager = Services.Get<MapManager>();
        soundManager = Services.Get<SoundManager>();
        beatManager = Services.Get<BeatManager>();

        Time.timeScale = 1f;

        soundManager.Init();
        mapManager.SetDeadlyTileSpawns(soundManager.CurrentSong.deadlyTileSpawns);
        StartCoroutine(StartMatch());
    }

    private IEnumerator StartMatch()
    {
        soundManager.PlayMusic();
        beatManager.ShouldPerformTicks = true;
        while (true)
        {
            var timeLeft = matchDurationSeconds - matchTimeElapsed;
            countdownTimer.UpdateTimeLeft(timeLeft);

            matchTimeElapsed += Time.deltaTime;

            if (matchTimeElapsed > 5f && AllPlayersDead())
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

    private bool AllPlayersDead()
    {
        List<PlayerState> states = playerManager.PlayerStates;
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].CurrentStateEnum != PlayerStateEnum.Dead)
                return false;
        }
        return true;
    }

    public void GameOver()
    {
        beatManager.ShouldPerformTicks = false;
        PauseGame(true);
        gameOverScreen.Show(gameResult);
        IsGameOver = true;
    }

    private void EvaluateGameOver()
    {
        var states = playerManager.PlayerStates;
        var p1Dead = states[0].CurrentStateEnum == PlayerStateEnum.Dead;
        var p2Dead = states[1].CurrentStateEnum == PlayerStateEnum.Dead;
        var p1ComboCount = states[0].ComboCounter;
        var p2ComboCount = states[1].ComboCounter;
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
