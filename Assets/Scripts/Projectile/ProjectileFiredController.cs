using System;
using System.Linq;
using UnityEngine;
using UnityEditor;


[Serializable]
public class LayerMaskReboundStrengthConfig
{
    public LayerMask LayerMask;
    public float minReboundRange;
    public float maxReboundRange;

    public float GetRandomReboundRange()
    {
        return UnityEngine.Random.Range(minReboundRange, maxReboundRange);
    }
}

[Serializable]
public class ProjectileFiredStateController : AProjectileStateController
{
    public event Action OnLandedAction;
    public event Action<string> OnBounce;

    [Range(0.1f, 5)] public float TimeOfTravel;

    protected override ProjectileState AnimationState { get; set; } = ProjectileState.Fired;
    [SerializeField] private AnimationCurve ScaleComponent;
    [SerializeField] private LayerMaskReboundStrengthConfig[] ReboundStrengthConfigs;

    [SerializeField] private float extraScale;
    [SerializeField] private float collisionActivationThreshold;
    private double travelFinishTime;
    private Vector2 velocity;
    private Vector3 initialScale;

    public override void OnDrawGizmos()
    {
        //Gizmos.DrawWireCube(this.transform.position, ComputeLapsedTimePercent() * Vector3.one);
    }
    public override void Init(Transform t, Animator a, CircleCollider2D c)
    {
        base.Init(t, a, c);
        this.initialScale = this.transform.localScale;

    }
    public override void Update()
    {
        if (velocity.magnitude != 0)
        {
            float progress = ComputeLapsedTimePercent();

            if (progress == 1)
            {
                Stop();
                OnLandedAction?.Invoke();
                return;
            }
            MoveProjectile();
            this.transform.localScale = Vector3.Lerp(initialScale, initialScale + Vector3.one * extraScale, ScaleComponent.Evaluate(progress));
            this.animationController.Play(AnimationState.ToString(), 0, progress);
        }
    }
    public void Fire(float travelDistance, int direction)
    {
        velocity = Vector2.right * direction * (travelDistance / TimeOfTravel);
        travelFinishTime = Time.time + TimeOfTravel;
    }
    public void Fire(float travelDistance, Vector2 direction)
    {
        velocity = direction * (travelDistance / TimeOfTravel);
        travelFinishTime = Time.time + TimeOfTravel;
    }

    public override void OnTriggerStay2D(Collider2D other)
    {
        if (ComputeLapsedTimePercent() >= collisionActivationThreshold)
        {
            if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
            {
                other.GetComponent<PlayerController>().StunPlayer();
                Bounce(other.gameObject.layer, UnityEngine.Random.insideUnitCircle.normalized);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Water"))
            {
                Vector2 randomVector;
                if (UnityEngine.Random.Range(0.0f, 1.0f) < 0.3)
                {
                    randomVector = UnityEngine.Random.insideUnitCircle;
                }
                else
                {
                    float coneAngle = 90.0f;
                    float randomAngle = UnityEngine.Random.Range(0, coneAngle);
                    randomAngle += 270 - coneAngle / 2.0f;
                    randomAngle *= Mathf.Deg2Rad;
                    randomVector = new Vector2(Mathf.Cos(randomAngle), Mathf.Sin(randomAngle));
                }

                Bounce(other.gameObject.layer, randomVector.normalized);
            }
            else if (other.gameObject.layer == LayerMask.NameToLayer("Root"))
            {
                Bounce(other.gameObject.layer, UnityEngine.Random.insideUnitCircle.normalized);
            }
        }
    }

    private void Bounce(int layer, Vector2 direction)
    {
        float randomReboundRange = ReboundStrengthConfigs.First(x => (1 < layer) & x.LayerMask != 0).GetRandomReboundRange();

        Fire(randomReboundRange, direction);
        OnBounce?.Invoke(LayerMask.LayerToName(layer));
    }
    private float ComputeLapsedTimePercent()
    {
        return 1.0f - Mathf.Max(0, (float)(travelFinishTime - Time.timeAsDouble) / TimeOfTravel);
    }

    private void MoveProjectile()
    {
        Vector2 position = this.transform.position;
        position = this.transform.position + (Vector3)velocity * Time.deltaTime;
        position = Camera.main.WorldToViewportPoint(position);
        position = position.Mod(Vector2.one);
        position += (Vector2.one - (position.Sign() * 2.0f - Vector2.one));
        position = Camera.main.ViewportToWorldPoint(position);

        this.transform.position = position;
    }
    private void Stop()
    {
        velocity = Vector2.zero;
        travelFinishTime = Time.timeAsDouble;
        this.transform.localScale = initialScale;
    }

}