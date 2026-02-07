using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SkinManager : MonoBehaviour
{
    [Header("Setup")]
    [Tooltip("Names of the Textures inside the 'Resources/Skins' folder")]
    [SerializeField] private List<string> skinTextureNames; 

    private Queue<string> _availableSkins;

    private void Awake()
    {
        Services.Register(this);
        InitializeSkins();
    }

    private void InitializeSkins()
    {
        // 1. Shuffle the list so every game is different
        var shuffled = skinTextureNames.OrderBy(x => Random.value).ToList();
        _availableSkins = new Queue<string>(shuffled);
    }

    public Sprite[] GetUniqueSkin()
    {
        if (_availableSkins.Count == 0)
        {
            Debug.LogWarning("SkinManager: No more unique skins available! Recycling.");
            InitializeSkins(); // Reshuffle if we run out
        }

        string skinName = _availableSkins.Dequeue();
        
        // Load all slices for this texture
        // IMPORTANT: The textures MUST be inside a folder named "Resources/Skins"
        Sprite[] sprites = Resources.LoadAll<Sprite>("Skins/" + skinName);

        if (sprites == null || sprites.Length == 0)
        {
            Debug.LogError($"SkinManager: Could not load sprites for '{skinName}'. Check folder path!");
            return null;
        }

        Debug.Log($"SkinManager: Assigned '{skinName}'");
        return sprites;
    }
}