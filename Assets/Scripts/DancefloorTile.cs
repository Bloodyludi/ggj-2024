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

    public float TileSize => ren.sprite.bounds.size.x;

    public void Init(BeatManager beatManager)
    {
        this.beatManager = beatManager;
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
            var t = (float)(1f - (beatManager.NextBeatTime - Time.timeAsDouble) / beatManager.BeatInterval);

            if (isDeadly)
            {
                ren.color = Color.Lerp(Color.white, Color.white * (0.8f - pulsatingJitter), t);
            }
            else
            {
                ren.color = Color.Lerp(Color.white * (0.8f - pulsatingJitter), Color.white, t);
            }

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