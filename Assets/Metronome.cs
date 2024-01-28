using System;
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
    public double NextBeatTime;
    private double preBeatTime;
    private double postBeatTime;
    private bool preBeatReady, beatReady, postBeatReady;

    public float gain = 0.5F;
    public int signatureHi = 4;
    public int signatureLo = 4;
    public bool playMetronomeTick = true;

    private double sampleRate = 0.0F;
    private int accent;
    public float SecondsPerBeat => 60.0f / bpm;
    public float LastBeatTime { get; set; }
    public int BeatCounter { get; set; }
    public double MoveWindowSeconds => MoveWindowTimePercent * beatInterval / 100;

    private void Start()
    {
        accent = signatureHi;
        sampleRate = AudioSettings.outputSampleRate;

        // Initialize beat timing
        beatInterval = 60.0 / bpm;
        NextBeatTime = AudioSettings.dspTime + beatInterval;
        CalculateBeatTimes();
    }

    private void Update()
    {
        double currentTime = AudioSettings.dspTime;

        UpdateBeatDebug(currentTime); // Update the cube's color

        if (currentTime >= preBeatTime && !preBeatReady)
        {
            OnPreBeat?.Invoke();
            preBeatReady = true;
        }

        if (currentTime >= NextBeatTime && !beatReady)
        {
            OnBeat?.Invoke();
            soundManager.PlaySfx(SoundManager.Sfx.DebugBeat3, 3);
            beatReady = true;
            LastBeatTime = Time.time;
            BeatCounter++;
        }

        if (currentTime >= postBeatTime && !postBeatReady)
        {
            OnPostBeat?.Invoke();
            postBeatReady = true;
        }
    }

    void OnAudioFilterRead(float[] data, int channels)
    {
        double currentTime = AudioSettings.dspTime;
        double samplesPerTick = sampleRate * 60.0 / bpm * 4.0 / signatureLo;

        if (currentTime >= NextBeatTime)
        {
            NextBeatTime += beatInterval;
            CalculateBeatTimes();

            preBeatReady = false;
            beatReady = false;
            postBeatReady = false;
        }

        if (playMetronomeTick)
        {
            GenerateMetronomeTick(data, channels, samplesPerTick);
        }
    }

    private void GenerateMetronomeTick(float[] data, int channels, double samplesPerTick)
    {
        double sample = AudioSettings.dspTime * sampleRate;
        for (int n = 0; n < data.Length / channels; n++, sample++)
        {
            float x = gain * Mathf.Sin((float)(2 * Math.PI * sample / samplesPerTick));
            for (int i = 0; i < channels; i++)
            {
                data[n * channels + i] += x;
            }
        }
    }
    
    private void UpdateBeatDebug(double currentTime)
    {
        var lapsedTimeSinceBeat = (float)(currentTime - LastBeatTime);
        var timeUntilNextBeat = (float)(NextBeatTime - currentTime);
        var timeToClosestBeat = Mathf.Min(lapsedTimeSinceBeat, timeUntilNextBeat);

        if (timeToClosestBeat <= MoveWindowSeconds)
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void CalculateBeatTimes()
    {
        preBeatTime = NextBeatTime - MoveWindowSeconds;
        postBeatTime = NextBeatTime + MoveWindowSeconds;
    }
}