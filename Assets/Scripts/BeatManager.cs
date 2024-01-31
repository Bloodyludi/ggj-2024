// using System;
// using System.Collections;
// using UnityEngine;
//
// public class BeatManager : MonoBehaviour
// {
//     public event Action OnPreBeat;
//     public event Action OnBeat;
//     public event Action OnPostBeat;
//
//     [SerializeField, Range(0, 120)] private float gameBPM = 120;
//     [SerializeField, Range(0.0f, 1.0f)] public float MoveWindowSeconds;
//     [Range(1, 100)] public float MoveWindowTimePercent = 10;
//
//     [SerializeField] private GameController gameController;
//     [SerializeField] private SoundManager soundManager;
//
//     private Coroutine BeatCoroutine;
//     [NonSerialized] public float LastBeatTime;
//     [NonSerialized] public float NextBeatTime;
//
//     public float BeatInterval;
//     public int BeatCounter { get; private set; }
//
//     [Header("Debug")] public bool ResetBeatCoroutine = false;
//     public bool PlayBeatDeBugSound = false;
//     [SerializeField] private GameObject BeatDebug;
//
//     private void OnGUI()
//     {
//         MoveWindowSeconds = MoveWindowTimePercent * BeatInterval / 100f;
//         if (ResetBeatCoroutine)
//         {
//             ResetBeatCoroutine = false;
//             InitBeatValues();
//             StopCoroutine(BeatCoroutine);
//             BeatCoroutine = StartCoroutine(StartBeatLoop());
//         }
//     }
//
//     public void Init()
//     {
//         InitBeatValues();
//         OnBeat -= DebugBeat;
//         OnBeat += DebugBeat;
//
//
//         BeatCoroutine = StartCoroutine(StartBeatLoop());
//     }
//
//     private void InitBeatValues()
//     {
//         BeatInterval = 60.0f / gameBPM;
//         NextBeatTime = Time.time + BeatInterval;
//         MoveWindowSeconds = MoveWindowTimePercent * BeatInterval / 100f;
//     }
//
//     public void DebugBeat()
//     {
//         if (PlayBeatDeBugSound)
//         {
//             //soundManager.PlaySfx(SoundManager.Sfx.DebugBeat3);
//         }
//     }
//
//     private IEnumerator StartBeatLoop()
//     {
//         while (gameController.IsGameOver == false)
//         {
//             OnBeat?.Invoke();
//
//             LastBeatTime = Time.time;
//             NextBeatTime = LastBeatTime + BeatInterval;
//             BeatCounter++;
//
//             yield return new WaitForSeconds(MoveWindowSeconds);
//
//             OnPostBeat?.Invoke();
//
//             yield return new WaitForSeconds(BeatInterval - MoveWindowSeconds * 2);
//
//             OnPreBeat?.Invoke();
//
//             yield return new WaitForSeconds(MoveWindowSeconds);
//         }
//
//         BeatCounter = 0;
//     }
//
//     public void Update()
//     {
//         var currentTime = Time.time;
//         var timeOfLastUpdate = Time.time - Time.deltaTime;
// /*
//         var preBeatTime = NextBeatTime - MoveWindowSeconds;
//         if (currentTime >= preBeatTime && timeOfLastUpdate <= preBeatTime)
//         {
//             OnPreBeat?.Invoke();
//             //soundManager.PlaySfx(SoundManager.Sfx.DebugBeat1);
//
//         }
//
//         if (currentTime >= NextBeatTime && timeOfLastUpdate <= NextBeatTime)
//         {
//             OnBeat?.Invoke();
//             soundManager.PlaySfx(SoundManager.Sfx.DebugBeat3);
//
//             LastBeatTime = currentTime;
//             NextBeatTime = LastBeatTime + BeatInterval;
//             BeatCounter++;
//         }
//
//         var postBeatTime = LastBeatTime + MoveWindowSeconds;
//         if (currentTime >= postBeatTime && timeOfLastUpdate <= postBeatTime)
//         {
//             OnPostBeat?.Invoke();
//             //soundManager.PlaySfx(SoundManager.Sfx.DebugBeat2);
//
//         }*/
//
//         // var currentTime = Time.time;
//         var lapsedTimeSinceBeat = currentTime - LastBeatTime;
//         var timeUntilNextBeat = NextBeatTime - currentTime;
//         var timeToClosestBeat = Mathf.Min(lapsedTimeSinceBeat, timeUntilNextBeat);
//         var moveWindowSeconds = MoveWindowTimePercent * BeatInterval / 100;
//
//         if (timeToClosestBeat <= moveWindowSeconds)
//         {
//             BeatDebug.GetComponent<Renderer>().material.color = Color.green;
//         }
//         else
//         {
//             BeatDebug.GetComponent<Renderer>().material.color = Color.red;
//         }
//     }
// }