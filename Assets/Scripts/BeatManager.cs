using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class BeatManager : MonoBehaviour
{
    public event Action OnPreBeat;
    public event Action OnBeat;
    public event Action OnPostBeat;

    [SerializeField] private GameObject beatDebug; // Reference to the cube
    [SerializeField] private SoundManager soundManager;
    [SerializeField, Range(0, 180)] private float bpm = 120.0f;
    [Range(1, 50)] public float MoveWindowTimePercent = 10;
    [SerializeField] public bool ShouldPerformTicks;
    private int lastBeatNumber;
    public float BeatInterval => 60.0f / bpm;
    public float LastBeatTime { get; set; }
    public float NextBeatTime { get; set; }
    public int BeatCounter { get; set; }

    public double MoveWindowSeconds => MoveWindowTimePercent * BeatInterval / 100;
    private GameController gameController;

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
        LastBeatTime = 0;

        NextBeatTime = LastBeatTime + BeatInterval;
    }

    public float GetCurrentTime()
    {
        return soundManager.MusicSource.timeSamples / (float)soundManager.MusicSource.clip.frequency;
    }

    public float GetCurrentBeatPosition()
    {
        return soundManager.MusicSource.timeSamples / (soundManager.MusicSource.clip.frequency * BeatInterval);
    }

    private void Awake()
    {
        this.gameController = FindObjectOfType<GameController>();
        OnBeat -=  ScaleDebugElement;
        OnBeat +=  ScaleDebugElement;
    }

    private void Update()
    {
        beatDebug.transform.localScale = Vector3.Lerp(beatDebug.transform.localScale, Vector3.one, Time.deltaTime*2f);
        if (gameController.IsGameOver)
        {
            return;
        }

        if (ShouldPerformTicks == false)
        {
            InitMetronome();
            return;
        }

        float currentTime = GetCurrentTime();
        float currentBeat = GetCurrentBeatPosition();
        if (Mathf.FloorToInt(currentBeat) != BeatCounter)
        {
            soundManager.PlaySfx(SoundManager.Sfx.DebugBeat3, 1);
            LastBeatTime = currentTime;
            NextBeatTime = LastBeatTime + BeatInterval;
            BeatCounter++;
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
        beatDebug.GetComponent<Renderer>().material.color = c;
    }

    public void ScaleDebugElement()
    {
        beatDebug.transform.localScale = Vector3.one * Random.Range(1.3f, 1.5f);
    }
}
