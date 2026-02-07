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
        Throw,
        Pickup // 1. Enum entry is correct
    }

    public AudioSource MusicSource;
    [SerializeField] private AudioSource sfxSource;

    private BeatManager beatManager;
    [SerializeField] private SongLevelData currentSong;

    [Header("SFX")]
    [SerializeField] private List<AudioClip> hitSounds;
    [SerializeField] private List<AudioClip> bounceWaterSounds;
    [SerializeField] private List<AudioClip> bounceRatSounds;
    [SerializeField] private List<AudioClip> bouncePlayerSounds;
    [SerializeField] private List<AudioClip> pullingSounds;
    [SerializeField] private List<AudioClip> pulledSounds;
    [SerializeField] private List<AudioClip> landingSounds;
    [SerializeField] private List<AudioClip> throwSounds;
    [SerializeField] private List<AudioClip> chargingSounds;
    
    // 2. ADD THIS: This creates the slot in the Inspector for your cheese sound
    [SerializeField] private List<AudioClip> pickupSounds; 

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
        
        // 3. ADD THIS: Maps the Enum to the list of clips
        sfxMap.Add(Sfx.Pickup, pickupSounds); 

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
        if (sfx != null) // Safety check
            sfxSource.PlayOneShot(sfx, volumeScale);
    }

    private AudioClip GetSfxClip(Sfx sound)
    {
        // Safety check to prevent errors if the list is empty
        if (!sfxMap.ContainsKey(sound) || sfxMap[sound].Count == 0)
        {
            return null;
        }

        var sounds = sfxMap[sound];
        return sounds[Random.Range(0, sounds.Count)];
    }
}