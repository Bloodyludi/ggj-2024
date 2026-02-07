using System.Collections;
using UnityEngine;

public class PickupItem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform visualTransform; 
    [SerializeField] private Transform shadowTransform; 

    [Header("Animation Settings")]
    [SerializeField] private float dropHeight = 15f;    
    [SerializeField] private float fallDuration = 0.5f; 
    [SerializeField] private float bounceHeight = 0.5f; 
    [SerializeField] private float bounceDuration = 0.2f;
    
    [Header("Visual Tuning")]
    [SerializeField] private float hoverHeight = 0.14f; 
    [SerializeField] private Vector2 gripOffset; 

    [Header("Beat Bounce")]
    [SerializeField] private float beatBouncePower = 0.2f; // How high it hops on beat

    // Logic State
    public Vector2Int GridPosition;
    public int ComboReward = 5;
    private BeatManager beatManager;
    private bool isLandingFinished = false;

    private void Start()
    {
        // Get the BeatManager via Services (matching coworker's style)
        beatManager = Services.Get<BeatManager>();
        
        transform.localPosition += (Vector3)gripOffset;
        StartCoroutine(AnimateEntrance());
    }

    private void Update()
    {
        // Only do the beat bounce after it has finished its initial landing
        if (isLandingFinished && beatManager != null && visualTransform != null)
        {
            // GetCurrentBeatPosition returns total beats (e.g. 12.5)
            // Using % 1.0f gives us just the fraction (0.5), which is our progress through the beat
            float progress = beatManager.GetCurrentBeatPosition() % 1.0f;
            
            // Mathf.Sin(progress * Mathf.PI) creates a hop that peaks mid-beat (0 -> 1 -> 0)
            float hop = Mathf.Sin(progress * Mathf.PI) * beatBouncePower;
            
            // Apply the hover height + the rhythmic hop
            visualTransform.localPosition = new Vector3(0, hoverHeight + hop, 0);

            // Optional: Scale the shadow slightly to match the height
            if (shadowTransform != null)
            {
                // Shadow shrinks slightly as the cheese gets higher
                float shadowScale = 1.0f - (hop * 0.4f); 
                shadowTransform.localScale = new Vector3(shadowScale, shadowScale, 1f);
            }
        }
    }

    private IEnumerator AnimateEntrance()
    {
        Vector3 targetLocalPos = new Vector3(0, hoverHeight, 0); 
        Vector3 startLocalPos = new Vector3(0, dropHeight, 0);
        
        if(shadowTransform) shadowTransform.localPosition = Vector3.zero;
        if(visualTransform) visualTransform.localPosition = startLocalPos;

        // --- FALL ---
        float elapsed = 0f;
        while (elapsed < fallDuration)
        {
            float t = elapsed / fallDuration;
            if(visualTransform) visualTransform.localPosition = Vector3.Lerp(startLocalPos, targetLocalPos, t * t);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // --- INITIAL IMPACT BOUNCE ---
        elapsed = 0f;
        while (elapsed < bounceDuration)
        {
            float t = elapsed / bounceDuration;
            float bounce = Mathf.Sin(t * Mathf.PI) * bounceHeight;
            if(visualTransform) visualTransform.localPosition = new Vector3(0, hoverHeight + bounce, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // --- ENABLE BEAT IDLE ---
        isLandingFinished = true;
    }

    public void OnCollected(PlayerController player)
    {
        var state = player.GetComponent<PlayerState>();
        if(state != null) state.ComboCounter += ComboReward;
        Destroy(gameObject);
    }
}