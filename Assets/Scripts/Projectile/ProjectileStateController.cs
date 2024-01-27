using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

public enum ProjectileState
{
    Grounded,
    Idle,
    Fired,
    Broken
}


public class ProjectileStateController : MonoBehaviour
{
    [HideInInspector] public event Action<ProjectileStateController, ProjectileState> OnStateChanged;

    [SerializeField] private Rigidbody2D rigigBody;
    [SerializeField] private CircleCollider2D collider2D;
    [SerializeField] private Animator projectileAnimator;

    [SerializeField] private ProjectileFiredStateController ProjectileFiredStateController;
    [SerializeField] private ProjectileGroundedStateController ProjectileGroundedStateController;

    private AProjectileStateController selectedStateController;
    private ProjectileState projectileState;

    private SoundManager soundManager;

    public void Start()
    {
        ActivateGround();
        soundManager = GameObject.FindWithTag("Sound")?.GetComponent<SoundManager>();
    }

    public void FireProjectile(float strength, int direction)
    {
        ActivateState(ProjectileState.Fired);
        ProjectileFiredStateController.Fire(strength, direction);
        ProjectileFiredStateController.OnLandedAction -= ActivateGround;
        ProjectileFiredStateController.OnLandedAction += ActivateGround;
        ProjectileFiredStateController.OnBounce -= PlayBounceSfx;
        ProjectileFiredStateController.OnBounce += PlayBounceSfx;
    }

    private void ActivateGround()
    {
        ActivateState(ProjectileState.Grounded);
    }

    private void PlayBounceSfx(string hitLayerName)
    {
        switch (hitLayerName)
        {
            case "Player":
                soundManager.PlaySfx(SoundManager.Sfx.CarrotBouncePlayer, 8f);
                break;
            case "Water":
                soundManager.PlaySfx(SoundManager.Sfx.CarrotBounceWater, 8f);
                break;
            case "Root":
            default:
                soundManager.PlaySfx(SoundManager.Sfx.CarrotBounceCarrot);
                break;
        }
    }

    private void ActivateState(ProjectileState newState)
    {
        if (projectileState == ProjectileState.Fired && newState == ProjectileState.Grounded)
        {
            soundManager.PlaySfx(SoundManager.Sfx.Landing);
        }

        projectileState = newState;
        OnStateChanged?.Invoke(this, projectileState);
        switch (projectileState)
        {
            case ProjectileState.Grounded:
                selectedStateController = ProjectileGroundedStateController;
                break;
            case ProjectileState.Fired:
                selectedStateController = ProjectileFiredStateController;
                break;
        }

        selectedStateController.Init(this.transform, this.projectileAnimator, this.collider2D);
    }

    public void Update()
    {
        selectedStateController?.Update();
    }

    public void OnDrawGizmos()
    {
        selectedStateController?.OnDrawGizmos();
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        selectedStateController?.OnTriggerEnter2D(other);
    }

    public void OnTriggerStay2D(Collider2D other)
    {
        selectedStateController?.OnTriggerStay2D(other);
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        selectedStateController?.OnTriggerExit2D(other);
    }
}