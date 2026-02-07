using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public enum Sfx
    {
        PlayerHit,
        BounceWater,
        BounceRat,
        BouncePlayer,
        Pulling,
        Pulled,
        Landing,
        Charging,
        Throw
    }

    public AudioSource MusicSource;
    [SerializeField] private AudioSource sfxSource;

    private BeatManager beatManager;
    [SerializeField] private SongLevelData currentSong;

    [Header("SFX")]
    [FormerlySerializedAs("carrotHit")]
    [SerializeField] private List<AudioClip> hitSounds;

    [FormerlySerializedAs("carrotBounceWater")]
    [SerializeField] private List<AudioClip> bounceWaterSounds;

    [FormerlySerializedAs("carrotBounceCarrot")]
    [SerializeField] private List<AudioClip> bounceRatSounds;

    [FormerlySerializedAs("carrotBouncePlayer")]
    [SerializeField] private List<AudioClip> bouncePlayerSounds;

    [FormerlySerializedAs("carrotPulling")]
    [SerializeField] private List<AudioClip> pullingSounds;

    [FormerlySerializedAs("carrotPulled")]
    [SerializeField] private List<AudioClip> pulledSounds;

    [FormerlySerializedAs("carrotLanding")]
    [SerializeField] private List<AudioClip> landingSounds;

    [FormerlySerializedAs("carrotThrow")]
    [SerializeField] private List<AudioClip> throwSounds;

    [FormerlySerializedAs("carrotCharging")]
    [SerializeField] private List<AudioClip> chargingSounds;

    private Dictionary<Sfx, List<AudioClip>> sfxMap = new();

    public SongLevelData CurrentSong => currentSong;

    private void Awake()
    {
        Services.Register(this);
    }

    public void Init(SongLevelData song = null)
    {
        beatManager = Services.Get<BeatManager>();
        sfxMap.Clear();
        sfxMap.Add(Sfx.PlayerHit, hitSounds);
        sfxMap.Add(Sfx.BounceWater, bounceWaterSounds);
        sfxMap.Add(Sfx.BounceRat, bounceRatSounds);
        sfxMap.Add(Sfx.BouncePlayer, bouncePlayerSounds);
        sfxMap.Add(Sfx.Pulling, pullingSounds);
        sfxMap.Add(Sfx.Pulled, pulledSounds);
        sfxMap.Add(Sfx.Landing, landingSounds);
        sfxMap.Add(Sfx.Throw, throwSounds);
        sfxMap.Add(Sfx.Charging, chargingSounds);

        if (song != null)
            currentSong = song;

        if (currentSong != null)
        {
            MusicSource.clip = currentSong.musicClip;
            MusicSource.loop = true;
            beatManager.SetBPM(currentSong.bpm);

            if (currentSong.moveWindowTimePercent > 0)
                beatManager.SetMoveWindowTimePercent(currentSong.moveWindowTimePercent);
        }
    }

    public void PlayMusic()
    {
        MusicSource.PlayDelayed(currentSong.startDelay);
    }


    public void PlaySfx(Sfx sound, float volumeScale = 3.5f)
    {
        var sfx = GetSfxClip(sound);
        sfxSource.PlayOneShot(sfx, volumeScale);
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
