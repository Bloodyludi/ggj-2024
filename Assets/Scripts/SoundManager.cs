using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
        Throw
    }

    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip gameSound;

    [Header("SFX")] [SerializeField] private List<AudioClip> carrotHit;
    [SerializeField] private List<AudioClip> carrotBounceWater;
    [SerializeField] private List<AudioClip> carrotBounceCarrot;
    [SerializeField] private List<AudioClip> carrotBouncePlayer;
    [SerializeField] private List<AudioClip> carrotPulling;
    [SerializeField] private List<AudioClip> carrotPulled;
    [SerializeField] private List<AudioClip> carrotLanding;
    [SerializeField] private List<AudioClip> carrotThrow;
    [SerializeField] private List<AudioClip> carrotCharging;

    private Dictionary<Sfx, List<AudioClip>> sfxMap = new();

    private void Awake()
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
    }

    private void Start()
    {
        musicSource.clip = gameSound;
        musicSource.loop = true;
        musicSource.Play();
    }

    public void SpeedUpMusic(float pitch)
    {
        musicSource.pitch = Mathf.Lerp(musicSource.pitch, pitch, 0.5f);
    }

    public AudioSource PlaySfxLoop(Sfx sound)
    {
        var sfx = GetSfxClip(sound);
        var sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.clip = sfx;
        sfxSource.loop = true;
        sfxSource.Play();

        return sfxSource;
    }

    public void StopSfxLoop(AudioSource source)
    {
        if (source == null)
        {
            return;
        }

        source.loop = false;
        source.Stop();
        Destroy(source);
    }

    public void PlaySfx(Sfx sound, float volumeScale = 3.5f)
    {
        var sfx = GetSfxClip(sound);
        musicSource.PlayOneShot(sfx, volumeScale);
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