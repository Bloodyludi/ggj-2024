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
    [SerializeField, Range(0, 120)] private float bpm = 120.0f;
    [Range(1, 100)] public float MoveWindowTimePercent = 10;

    private double beatInterval;
    private bool  beatReady;

    public float SecondsPerBeat => 60.0f / bpm;
    public float LastBeatTime { get; set; }
    public int BeatCounter { get; set; }
    public double MoveWindowSeconds => MoveWindowTimePercent * beatInterval / 100;
    [HideInInspector] public double NextBeatTime;

    private void Start()
    {

        // Initialize beat timing
        beatInterval = 60.0 / bpm;
        NextBeatTime = AudioSettings.dspTime + beatInterval;
    }

    private void Update()
    {
        double currentTime = AudioSettings.dspTime;

        UpdateBeatDebug(currentTime); // Update the cube's color

        if (currentTime >= NextBeatTime && !beatReady)
        {
            OnBeat?.Invoke();
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


    void OnAudioFilterRead(float[] data, int channels)
    {
        double currentTime = AudioSettings.dspTime;

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