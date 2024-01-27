using System;
using System.Collections;
using UnityEngine;

public class BeatManager : MonoBehaviour
{
    public event Action OnBeatUpdate;

    [SerializeField] private float gameBPM = 120;
    [SerializeField] private GameObject BeatDebug;
    [SerializeField] private GameController gameController;
    [SerializeField] public float moveWindowTimePercent = 10;

    [NonSerialized] public float lastBeatTime;
    [NonSerialized] public float nextBeatTime;
    public float SecondsPerBeat => 60.0f / gameBPM;

    public void Init()
    {
        StartCoroutine(StartBeatLoop());
        // StartCoroutine(BeatDebugDisplay());
    }

    private IEnumerator StartBeatLoop()
    {
        while (gameController.IsGameOver == false)
        {
            Debug.Log($"Beat!");
            if (OnBeatUpdate != null)
            {
                OnBeatUpdate();
            }
            
            lastBeatTime = Time.time;
            nextBeatTime = lastBeatTime + SecondsPerBeat;
            
            yield return new WaitForSeconds(SecondsPerBeat);
            // StartCoroutine(BeatDebugDisplay());
        }
    }

    // private IEnumerator BeatDebugDisplay()
    // {
    //     BeatDebug.transform.localScale = Vector3.one * 2;
    //
    //     for (float i = 0; i < SecondsPerBeat; i += Time.deltaTime)
    //     {
    //         float percent = Mathf.Clamp01(i / SecondsPerBeat);
    //         percent = 1 - percent;
    //         percent = percent * 2 - 1;
    //         BeatDebug.transform.localScale = Vector3.one + Vector3.one * percent;
    //         yield return new WaitForEndOfFrame();
    //     }
    //
    //     BeatDebug.transform.localScale = Vector3.one * 2;
    // }

    public void Update()
    {
        var currentTime = Time.time;
        var lapsedTimeSinceBeat = currentTime - lastBeatTime;
        var timeUntilNextBeat = nextBeatTime - currentTime;
        var timeToClosestBeat = Mathf.Min(lapsedTimeSinceBeat, timeUntilNextBeat);
        var moveWindowSeconds = moveWindowTimePercent * SecondsPerBeat / 100;
        
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