using System;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

public class SongSelector : MonoBehaviour
{
    [SerializeField] private SongLibrary songLibrary;
    [SerializeField] private TMP_Text songNameText;

    public event Action<SongLevelData> OnSongChanged;

    private int currentIndex;

    private void Start()
    {
        if (songLibrary == null || songLibrary.songs == null || songLibrary.songs.Length == 0)
            return;

        currentIndex = 0;
        ApplySelection();
    }

    [UsedImplicitly]
    public void NextSong()
    {
        if (songLibrary == null || songLibrary.songs.Length == 0) return;

        currentIndex = (currentIndex + 1) % songLibrary.songs.Length;
        ApplySelection();
    }

    [UsedImplicitly]
    public void PreviousSong()
    {
        if (songLibrary == null || songLibrary.songs.Length == 0) return;

        currentIndex = (currentIndex - 1 + songLibrary.songs.Length) % songLibrary.songs.Length;
        ApplySelection();
    }

    private void ApplySelection()
    {
        var song = songLibrary.songs[currentIndex];
        SongLibrary.SelectedSong = song;

        if (songNameText != null)
            songNameText.text = "Song: " + song.DisplayName;

        OnSongChanged?.Invoke(song);
    }
}
