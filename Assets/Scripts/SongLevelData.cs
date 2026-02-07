using UnityEngine;

[CreateAssetMenu(fileName = "NewSongLevel", menuName = "Lab Rats/Song Level Data")]
public class SongLevelData : ScriptableObject
{
    public string displayName;
    public AudioClip musicClip;
    public int bpm = 120;
    public float startDelay;
    [Tooltip("Per-song beat window override (%). -1 = use BeatManager default (10%).")]
    public float moveWindowTimePercent = -1;
    public TileSpawnConfig[] deadlyTileSpawns;

    [Header("Pickup Settings")]
    public float pickupSpawnInterval = 5.0f;
    public int pickupComboReward = 5;

    public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
}
