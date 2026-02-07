using UnityEngine;

[CreateAssetMenu(fileName = "NewSongLevel", menuName = "Lab Rats/Song Level Data")]
public class SongLevelData : ScriptableObject
{
    public AudioClip musicClip;
    public int bpm = 120;
    public float startDelay;
    public TileSpawnConfig[] deadlyTileSpawns;
}
