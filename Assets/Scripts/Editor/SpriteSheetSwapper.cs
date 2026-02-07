using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SpriteSheetSwapper : MonoBehaviour
{
    [Header("Skin Configuration")]
    [SerializeField] private List<SkinData> availableSkins; 
    
    [Header("References")]
    [Tooltip("Drag the 'playerref' or 'player2ref' object here")]
    [SerializeField] private SpriteRenderer reflectionRenderer; 

    private SpriteRenderer _mainRenderer;
    private Dictionary<string, Sprite> _spriteSheetCache;

    // A static "Deck" of indices shared by all players to ensure unique skins
    private static List<int> _shuffledIndices;

    private void Awake()
    {
        _mainRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        int count = availableSkins != null ? availableSkins.Count : 0;
        
        if (count == 0) return;

        // 1. Initialize deck if needed
        if (_shuffledIndices == null || _shuffledIndices.Count == 0)
        {
            InitializeDeck(count);
        }

        // 2. Pick a unique card from the deck
        int skinIndex = _shuffledIndices[0];
        _shuffledIndices.RemoveAt(0); 

        // 3. Load the skin for BOTH renderers
        LoadSkin(availableSkins[skinIndex]);
    }

    private void InitializeDeck(int count)
    {
        _shuffledIndices = new List<int>();
        for (int i = 0; i < count; i++) _shuffledIndices.Add(i);
        _shuffledIndices = _shuffledIndices.OrderBy(x => Random.value).ToList();
    }

    private void LoadSkin(SkinData data)
    {
        if (data == null || data.sprites == null) return;

        _spriteSheetCache = new Dictionary<string, Sprite>();
        
        foreach (var sprite in data.sprites)
        {
            string originalName = sprite.name;

            // Register the exact name
            if (!_spriteSheetCache.ContainsKey(originalName))
                _spriteSheetCache.Add(originalName, sprite);

            // Register the "Player 2" equivalent (Mapping _1_ to _2_)
            if (originalName.Contains("_1_"))
            {
                string p2Name = originalName.Replace("_1_", "_2_");
                if (!_spriteSheetCache.ContainsKey(p2Name))
                    _spriteSheetCache.Add(p2Name, sprite);
            }
        }
    }

    private void LateUpdate()
    {
        if (_spriteSheetCache == null) return;

        // 1. Swap the Main Body
        if (_mainRenderer.sprite != null && _spriteSheetCache.TryGetValue(_mainRenderer.sprite.name, out Sprite mainSwap))
        {
            _mainRenderer.sprite = mainSwap;
        }

        // 2. Swap the Reflection (using the exact same skin cache)
        if (reflectionRenderer != null && reflectionRenderer.sprite != null)
        {
            if (_spriteSheetCache.TryGetValue(reflectionRenderer.sprite.name, out Sprite refSwap))
            {
                reflectionRenderer.sprite = refSwap;
            }
        }
    }
}