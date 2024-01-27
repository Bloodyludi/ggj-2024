using UnityEngine;

public class DancefloorTile : MonoBehaviour
{
    public Sprite darkSprite;
    public Sprite brightSprite;
    public SpriteRenderer ren;
    
    private int x;
    private int y;
    
    public float TileSize => ren.sprite.bounds.size.x;


    public void SetPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
        transform.localPosition = new Vector3(x, y) * TileSize;
        ren.sprite = (x + y) % 2 == 0 ? darkSprite : brightSprite;
    }
}
