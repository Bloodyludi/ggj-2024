using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PickupTargetSensor : MonoBehaviour
{
    public bool HasPickupTarget => CurrentPickupTarget != null;
    
    public Collider2D CurrentPickupTarget { get; private set; }
    
    private readonly List<Collider2D> targetsInRange = new();

    protected void OnValidate()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (targetsInRange.Contains(other)) return;
        
        targetsInRange.Add(other);
        
        UpdateCurrentInteractable();
    }

    protected virtual void OnTriggerExit2D(Collider2D other)
    {
        if (targetsInRange.Contains(other))
        {
            targetsInRange.Remove(other);
        }
        
        UpdateCurrentInteractable();
    }

    private void UpdateCurrentInteractable()
    {
        if (targetsInRange.Count > 0)
        {
            Collider2D closest = null;
            var closestDistance = float.MaxValue;

            foreach (var t in targetsInRange)
            {
                var dist = Vector2.Distance(t.transform.position, transform.position);
                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closest = t;
                }
            }
            
            CurrentPickupTarget = closest;
        }
        else
        {
            CurrentPickupTarget = null;
        }
    }
}
