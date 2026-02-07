using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class DancefloorTile : MonoBehaviour
{
    public Sprite darkSprite;
    public Sprite brightSprite;
    public Sprite deadlySprite;
    public SpriteRenderer ren;

    public bool isDeadly;

    public Vector2Int position = new(0, 0);
    [NonSerialized] public Vector2Int movementDirection;
    private BeatManager beatManager;
    private float pulsatingJitter;

    // --- NEW: Proximity Glow State ---
    private Color proximityColor = Color.black;

    public float TileSize => ren.sprite.bounds.size.x;

    public void Init(BeatManager beatManager)
    {
        this.beatManager = beatManager;
    }

    // --- NEW: Set intensity of red bleed ---
    public void SetGlow(float intensity)
    {
        // Blends between black (no effect) and pure red
        proximityColor = Color.Lerp(Color.black, new Color(1f, 0f, 0f, 0f), intensity);
    }

    public void SetDeadly(bool deadly)
    {
        isDeadly = deadly;
        UpdateSprite();
    }

    public void SetPosition(int x, int y)
    {
        position = new Vector2Int(x, y);
        transform.localPosition = new Vector3(x, y) * TileSize;
        UpdateSprite();

        StartCoroutine(StartPulsing());
    }

    private IEnumerator StartPulsing()
    {
        pulsatingJitter = Random.Range(0f, isDeadly ? 0.1f : 0.3f);

        while (true)
        {
            if (beatManager == null)
            {
                yield return null;
                continue;
            }

            var t = (float)(1f - (beatManager.NextBeatTime - beatManager.GetCurrentTime()) / beatManager.BeatInterval);

            Color baseColor;
            if (isDeadly)
            {
                baseColor = Color.Lerp(Color.white, Color.white * (0.8f - pulsatingJitter), t);
            }
            else
            {
                baseColor = Color.Lerp(Color.white * (0.8f - pulsatingJitter), Color.white, t);
            }

            // --- FIXED: Add the base beat color to the proximity glow ---
            ren.color = baseColor + proximityColor;

            if (t >= 0.96f)
            {
                pulsatingJitter = Random.Range(0f, isDeadly ? 0.1f : 0.3f);
            }

            yield return null;
        }
    }

    private void UpdateSprite()
    {
        if (isDeadly)
        {
            ren.sprite = deadlySprite;
        }
        else
        {
            ren.sprite = (position.x + position.y) % 2 == 0 ? darkSprite : brightSprite;
        }
    }
}