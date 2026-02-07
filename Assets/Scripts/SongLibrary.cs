using UnityEngine;

[CreateAssetMenu(fileName = "SongLibrary", menuName = "Lab Rats/Song Library")]
public class SongLibrary : ScriptableObject
{
    public SongLevelData[] songs;

    public static SongLevelData SelectedSong { get; set; }
}
