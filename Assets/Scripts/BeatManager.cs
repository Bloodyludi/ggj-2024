using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    public event Action OnPreBeat;
    public event Action OnBeat;
    public event Action OnPostBeat;

    [SerializeField] private GameObject beatDebug;
    private SoundManager soundManager;
    [SerializeField, Range(0, 180)] private float bpm = 120.0f;
    [Range(1, 50)] public float MoveWindowTimePercent = 10;

    public bool ShouldPerformTicks;

    public float BeatInterval => 60.0f / bpm;
    public float LastBeatTime { get; set; }
    public float NextBeatTime { get; set; }
    public int BeatCounter { get; set; }

    public double MoveWindowSeconds => MoveWindowTimePercent * BeatInterval / 100;

    private GameController gameController;

    private void Awake()
    {
        Services.Register(this);
        OnBeat -= ScaleDebugElement;
        OnBeat += ScaleDebugElement;
    }

    private void Start()
    {
        soundManager = Services.Get<SoundManager>();
        gameController = Services.Get<GameController>();
        InitMetronome();
    }

    public void SetBPM(int bpm)
    {
        this.bpm = bpm;
    }

    public void SetMoveWindowTimePercent(float percent)
    {
        MoveWindowTimePercent = percent;
    }

    private void InitMetronome()
    {
        LastBeatTime = 0;
        NextBeatTime = LastBeatTime + BeatInterval;
    }

    public float GetCurrentTime()
    {
        if (soundManager == null || soundManager.MusicSource == null || soundManager.MusicSource.clip == null)
            return Time.time;

        return soundManager.MusicSource.timeSamples / (float)soundManager.MusicSource.clip.frequency;
    }

    public float GetCurrentBeatPosition()
    {
        if (soundManager == null || soundManager.MusicSource == null || soundManager.MusicSource.clip == null)
            return 0;

        return soundManager.MusicSource.timeSamples / (soundManager.MusicSource.clip.frequency * BeatInterval);
    }

    private void Update()
    {
        if (beatDebug != null)
            beatDebug.transform.localScale = Vector3.Lerp(beatDebug.transform.localScale, Vector3.one, Time.deltaTime * 2f);

        if (gameController == null || gameController.IsGameOver)
        {
            return;
        }

        if (ShouldPerformTicks == false)
        {
            InitMetronome();
            return;
        }

        if (soundManager == null || soundManager.MusicSource == null || !soundManager.MusicSource.isPlaying)
        {
            return;
        }

        float currentTime = GetCurrentTime();
        float currentBeat = GetCurrentBeatPosition();

        if (Mathf.FloorToInt(currentBeat) != BeatCounter)
        {
            LastBeatTime = currentTime;
            NextBeatTime = LastBeatTime + BeatInterval;
            BeatCounter = Mathf.FloorToInt(currentBeat);
            OnBeat?.Invoke();
            StartCoroutine(ScheduleAction(OnPreBeat, (float)(BeatInterval - MoveWindowSeconds)));
            StartCoroutine(ScheduleAction(OnPostBeat, (float)MoveWindowSeconds));
        }
    }

    private IEnumerator ScheduleAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    public void DebugBeatRange(Color c)
    {
        if (beatDebug != null)
            beatDebug.GetComponent<Renderer>().material.color = c;
    }

    public void ScaleDebugElement()
    {
        if (beatDebug != null)
            beatDebug.transform.localScale = Vector3.one * UnityEngine.Random.Range(1.3f, 1.5f);
    }
}
