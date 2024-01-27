using System;
using System.Collections;
using UnityEngine;


public class BeatManager : MonoBehaviour
{
    public event Action OnBeatUpdate;

    [SerializeField] private float gameBPM = 120;
    [SerializeField] private GameObject BeatDebug;
    [SerializeField] private GameController gameController;

    public float Tempo => 60.0f / gameBPM;

    public void Init()
    {
        StartCoroutine(StartBeatLoop());
        StartCoroutine(BeatDebugDisplay());
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
            yield return new WaitForSeconds(Tempo);
            StartCoroutine(BeatDebugDisplay());
        }
    }

    private IEnumerator BeatDebugDisplay()
    {
        BeatDebug.transform.localScale = Vector3.one * 2;

        for (float i = 0; i < Tempo; i += Time.deltaTime)
        {
            float percent = Mathf.Clamp01(i / Tempo);
            percent = 1 - percent;
            percent = percent * 2 - 1;
            BeatDebug.transform.localScale = Vector3.one + Vector3.one * percent;
            yield return new WaitForEndOfFrame();
        }

        BeatDebug.transform.localScale = Vector3.one * 2;
    }
}