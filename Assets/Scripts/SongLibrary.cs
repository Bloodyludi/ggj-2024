using UnityEngine;

[CreateAssetMenu(fileName = "SongLibrary", menuName = "Lab Rats/Song Library")]
public class SongLibrary : ScriptableObject
{
    public SongLevelData[] songs;
}
