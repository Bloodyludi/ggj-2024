using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    public event Action OnPreBeat;
    public event Action OnBeat;
    public event Action OnPostBeat;

    [SerializeField] private GameObject BeatDebug; // Reference to the cube
    [SerializeField] private SoundManager soundManager;
    [SerializeField, Range(0, 180)] private float bpm = 120.0f;
    [Range(1, 100)] public float MoveWindowTimePercent = 10;
    [SerializeField] public bool shouldPerformTicks;
    private double beatInterval;
    private bool beatReady;

    public float SecondsPerBeat => 60.0f / bpm;
    public float LastBeatTime { get; set; }
    public int BeatCounter { get; set; }
    public double MoveWindowSeconds => MoveWindowTimePercent * beatInterval / 100;
    [HideInInspector] public double NextBeatTime;
    private GameController gameController;
    private double TimeToStart;


    private void Start()
    {
        InitMetronome();
    }

    public void SetBPM(int bpm)
    {
        this.bpm = bpm;
    }

    private void InitMetronome()
    {
        TimeToStart = Time.timeAsDouble;
        beatInterval = 60.0 / bpm;
        NextBeatTime = GetCurrentTime() + beatInterval;
    }

    private void Awake()
    {
        this.gameController = FindObjectOfType<GameController>();
    }

    private void Update()
    {
        if (gameController.IsGameOver)
        {
            return;
        }

        if (shouldPerformTicks == false)
        {
            InitMetronome();
            return;
        }

        double currentTime = GetCurrentTime();

        UpdateBeatDebug(currentTime);
        DoAccurateBeat(currentTime);
    }

    private void DoAccurateBeat(double currentTime)
    {
        if (currentTime >= NextBeatTime && !beatReady)
        {
            OnBeat?.Invoke();
            soundManager.PlaySfx(SoundManager.Sfx.DebugBeat3, 1);
            LastBeatTime = Time.time;
            BeatCounter++;
            beatReady = true;

            StartCoroutine(ScheduleAction(OnPreBeat, (float)(beatInterval - MoveWindowSeconds * 0.5f)));
            StartCoroutine(ScheduleAction(OnPostBeat, (float)MoveWindowSeconds * 0.5f));
        }
    }

    private IEnumerator ScheduleAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public double GetCurrentTime()
    {
#if !UNITY_WEBGL
        return AudioSettings.dspTime;
#else
        return Time.timeAsDouble - TimeToStart;
#endif
    }

#if !UNITY_WEBGL
    void OnAudioFilterRead(float[] data, int channels)
    {
#else
    public void FixedUpdate()
    {
#endif
        double currentTime = GetCurrentTime();

        if (currentTime >= NextBeatTime)
        {
            // Advance to the next beat
            NextBeatTime += beatInterval;
            beatReady = false;
        }
    }

    private void UpdateBeatDebug(double currentTime)
    {
        var lapsedTimeSinceBeat = (float)(currentTime - LastBeatTime);
        var timeUntilNextBeat = (float)(NextBeatTime - currentTime);
        var timeToClosestBeat = Mathf.Min(lapsedTimeSinceBeat, timeUntilNextBeat);

        if (timeToClosestBeat <= MoveWindowSeconds * 0.5f)
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}