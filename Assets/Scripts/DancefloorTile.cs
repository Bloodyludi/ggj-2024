using System;
using System.Collections;
using UnityEngine;

public class DancefloorTile : MonoBehaviour
{
    public Sprite darkSprite;
    public Sprite brightSprite;
    public Sprite deadlySprite;
    public SpriteRenderer ren;
    
    public bool isDeadly;

    public Vector2Int position = new(0,0);
    [NonSerialized] public Vector2Int movementDirection;
    private BeatManager beatManager;

    public float TileSize => ren.sprite.bounds.size.x;

    private void Awake()
    {
        beatManager = GameObject.FindObjectOfType<BeatManager>();
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
        while (true)
        {
            var t = (float)(1f - (beatManager.NextBeatTime - AudioSettings.dspTime) / beatManager.SecondsPerBeat);

            if (isDeadly)
            {
                ren.color = Color.Lerp(Color.grey, Color.white, t);
            }
            else
            {
                ren.color = Color.Lerp(Color.white, Color.grey, t);
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
