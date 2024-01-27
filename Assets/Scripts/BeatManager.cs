using System;
using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public event Action OnBeatUpdate;

    [SerializeField] private float gameBPM = 120;
    [SerializeField] private GameObject BeatDebug;
    [SerializeField] private GameController gameController;

    [Range(1,100)] public float MoveWindowTimePercent = 10;
    [NonSerialized] public float LastBeatTime;
    [NonSerialized] public float NextBeatTime;
    public float SecondsPerBeat => 60.0f / gameBPM;
    public int BeatCounter { get; private set; }

    public void Init()
    {
        StartCoroutine(StartBeatLoop());
    }

    private IEnumerator StartBeatLoop()
    {
        while (gameController.IsGameOver == false)
        {
            Debug.Log($"Beat!");
            if (OnBeatUpdate != null)
            {
                OnBeatUpdate.Invoke();
            }

            LastBeatTime = Time.time;
            NextBeatTime = LastBeatTime + SecondsPerBeat;
            BeatCounter++;

            yield return new WaitForSeconds(SecondsPerBeat);
        }

        BeatCounter = 0;
    }

    public void Update()
    {
        var currentTime = Time.time;
        var lapsedTimeSinceBeat = currentTime - LastBeatTime;
        var timeUntilNextBeat = NextBeatTime - currentTime;
        var timeToClosestBeat = Mathf.Min(lapsedTimeSinceBeat, timeUntilNextBeat);
        var moveWindowSeconds = MoveWindowTimePercent * SecondsPerBeat / 100;

        if (timeToClosestBeat <= moveWindowSeconds)
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            BeatDebug.GetComponent<Renderer>().material.color = Color.red;
        }
    }
}