using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void OnDisable()
    {
        GameObject.Destroy(this);
    }
}
