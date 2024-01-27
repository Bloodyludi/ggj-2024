using UnityEngine;
using System;

[Serializable]
public class ProjectileGroundedStateController : AProjectileStateController
{

    protected override ProjectileState AnimationState { get; set; } = ProjectileState.Grounded;

    [SerializeField] AnimationCurve ProximitySpeedCurve;

    public override void Update()
    {

    }
    public override void OnDrawGizmos()
    {
    }
    public override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interaction"))
        {
            SpeedUpAnimation(other);
        }
    }

    public override void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interaction"))
        {
            SpeedUpAnimation(other);
        }
    }

    public override void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Interaction"))
        {
            this.animationController.speed = 1;
        }

    }

    private void SpeedUpAnimation(Collider2D other)
    {
        float maxLength = ((CircleCollider2D)other).radius + this.collider.radius;
        float distancePercent = (other.transform.position - this.transform.position).magnitude / maxLength;
        this.animationController.speed = 1 + ProximitySpeedCurve.Evaluate(1 - distancePercent);
    }


}