using UnityEngine;

public class AutoDestroy : MonoBehaviour
{
    private void OnDisable()
    {
        if (!gameObject.scene.isLoaded) return;
        GameObject.Destroy(this.gameObject);
    }
}
