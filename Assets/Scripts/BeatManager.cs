using System;
using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public event Action OnPreBeat;
    public event Action OnBeat;
    public event Action OnPostBeat;

    [SerializeField] private float gameBPM = 120;
    [SerializeField] private GameObject BeatDebug;
    [SerializeField] private GameController gameController;

    [Range(1,100)] public float MoveWindowTimePercent = 10;
    [NonSerialized] public float LastBeatTime;
    [NonSerialized] public float NextBeatTime;
    public float SecondsPerBeat => 60.0f / gameBPM;
    public float MoveWindowSeconds => MoveWindowTimePercent * SecondsPerBeat / 100f;
    public int BeatCounter { get; private set; }
    

    public void Init()
    {
        NextBeatTime = Time.time + SecondsPerBeat;
        
        // StartCoroutine(StartBeatLoop());
    }

    // private IEnumerator StartBeatLoop()
    // {
    //     while (gameController.IsGameOver == false)
    //     {
    //         OnBeat?.Invoke();
    //
    //         LastBeatTime = Time.time;
    //         NextBeatTime = LastBeatTime + SecondsPerBeat;
    //         BeatCounter++;
    //
    //         yield return new WaitForSeconds(MoveWindowSeconds);
    //
    //         OnPostBeat?.Invoke();
    //
    //         yield return new WaitForSeconds(SecondsPerBeat - MoveWindowSeconds * 2);
    //         
    //         OnPreBeat?.Invoke();
    //         
    //         yield return new WaitForSeconds(MoveWindowSeconds);
    //     }
    //
    //     BeatCounter = 0;
    // }

    public void Update()
    {
        var currentTime = Time.time;
        var timeOfLastUpdate = Time.time - Time.deltaTime;

        var preBeatTime = NextBeatTime - MoveWindowSeconds;
        if (currentTime >= preBeatTime && timeOfLastUpdate <= preBeatTime)
        {
            OnPreBeat?.Invoke();
            
        }
        
        if (currentTime >= NextBeatTime && timeOfLastUpdate <= NextBeatTime)
        {
            OnBeat?.Invoke();
            
            LastBeatTime = currentTime;
            NextBeatTime = LastBeatTime + SecondsPerBeat;
            BeatCounter++;
        }
        
        var postBeatTime = LastBeatTime + MoveWindowSeconds;
        if (currentTime >= postBeatTime && timeOfLastUpdate <= postBeatTime)
        {
            OnPostBeat?.Invoke();
        }
        
        // var currentTime = Time.time;
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