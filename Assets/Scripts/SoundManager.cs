using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class SyncedSounds
{
    public AudioClip music;
    public float delay;
    public int bpm;
}

public enum SongDifficulty
{
    easyPeasy = 0,
    ratDance = 1,
    pesticide = 2,
    ratZap = 3
}

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public enum Sfx
    {
        PlayerHit,
        CarrotBounceWater,
        CarrotBounceCarrot,
        CarrotBouncePlayer,
        Pulling,
        Pulled,
        Landing,
        Charging,
        Throw,
        DebugBeat1,
        DebugBeat2,
        DebugBeat3
    }

    public AudioSource MusicSource;

    [SerializeField] private BeatManager beatManager;
    [SerializeField] private List<SyncedSounds> gameSounds;

    [Header("SFX")] [SerializeField] private List<AudioClip> carrotHit;
    [SerializeField] private List<AudioClip> carrotBounceWater;
    [SerializeField] private List<AudioClip> carrotBounceCarrot;
    [SerializeField] private List<AudioClip> carrotBouncePlayer;
    [SerializeField] private List<AudioClip> carrotPulling;
    [SerializeField] private List<AudioClip> carrotPulled;
    [SerializeField] private List<AudioClip> carrotLanding;
    [SerializeField] private List<AudioClip> carrotThrow;
    [SerializeField] private List<AudioClip> carrotCharging;

    [SerializeField] private List<AudioClip> debugBeat1;
    [SerializeField] private List<AudioClip> debugBeat2;
    [SerializeField] private List<AudioClip> debugBeat3;

    private SyncedSounds currentGameSound;
    private Dictionary<Sfx, List<AudioClip>> sfxMap = new();

    public void Init(int musicIndex = 0)
    {
        sfxMap.Clear();
        sfxMap.Add(Sfx.PlayerHit, carrotHit);
        sfxMap.Add(Sfx.CarrotBounceWater, carrotBounceWater);
        sfxMap.Add(Sfx.CarrotBounceCarrot, carrotBounceCarrot);
        sfxMap.Add(Sfx.CarrotBouncePlayer, carrotBouncePlayer);
        sfxMap.Add(Sfx.Pulling, carrotPulling);
        sfxMap.Add(Sfx.Pulled, carrotPulled);
        sfxMap.Add(Sfx.Landing, carrotLanding);
        sfxMap.Add(Sfx.Throw, carrotThrow);
        sfxMap.Add(Sfx.Charging, carrotCharging);
        sfxMap.Add(Sfx.DebugBeat1, debugBeat1);
        sfxMap.Add(Sfx.DebugBeat2, debugBeat2);
        sfxMap.Add(Sfx.DebugBeat3, debugBeat3);


        if (gameSounds.Count != 0)
        {
            currentGameSound = gameSounds[musicIndex];
            MusicSource.clip = currentGameSound.music;
            MusicSource.loop = true;
            beatManager.SetBPM(currentGameSound.bpm);
        }
    }

    public void PlayMusic()
    {
        MusicSource.PlayDelayed(currentGameSound.delay);
    }


    public void PlaySfx(Sfx sound, float volumeScale = 3.5f)
    {
        var sfx = GetSfxClip(sound);
        MusicSource.PlayOneShot(sfx, volumeScale);
    }

    private AudioClip GetSfxClip(Sfx sound)
    {
        if (sfxMap[sound].Count == 0)
        {
            return null;
        }

        var sounds = sfxMap[sound];
        return sounds[Random.Range(0, sounds.Count)];
    }
}