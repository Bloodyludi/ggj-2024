using UnityEngine;

public static class Vector2Extensions
{
    public static Vector2 Mod(this Vector2 v, Vector2 m)
    {
        v.x %= m.x;
        v.y %= m.y;
        return v;
    }

    public static Vector2 Sign(this Vector2 v)
    {
        v.x = Mathf.Sign(v.x);
        v.y = Mathf.Sign(v.y);
        return v;
    }

    public static Vector2 Abs(this Vector2 v)
    {
        v.x = Mathf.Abs(v.x);
        v.y = Mathf.Abs(v.y);
        return v;
    }
    public static Vector2 ScaleVector(this Vector2 v, Vector2 a)
    {
        v.x *= a.x;
        v.y *= a.y;
        return v;
    }
}