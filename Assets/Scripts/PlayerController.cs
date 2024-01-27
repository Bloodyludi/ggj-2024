using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PickupTargetSensor pickupTargetSensor;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float maxThrowDistance = 10f;
    [SerializeField] private float durationToReachMaxDistance = 2f;
    [SerializeField] private float stunDuration = 1.0f;

    private Vector2 moveDir = Vector2.zero;
    private float currentThrowCharge;
    private Coroutine chargeThrowRoutine;
    private SoundManager soundManager;
    private AudioSource pullingLoop;
    private AudioSource chargingLoop;

    public void StunPlayer()
    {
        if (playerState.CurrentAction == PlayerAction.Stunned) return;
        soundManager.PlaySfx(SoundManager.Sfx.PlayerHit);
        StartCoroutine(PlayerStunned());
    }

    private IEnumerator PlayerStunned()
    {
        var currentAction = playerState.CurrentAction;
        playerState.CurrentAction = PlayerAction.Stunned;
        yield return new WaitForSeconds(stunDuration);
        playerState.CurrentAction = currentAction;
    }

    private void Awake()
    {
        soundManager = GameObject.FindWithTag("Sound")?.GetComponent<SoundManager>();
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += EventHandler;
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= EventHandler;
    }

    private void EventHandler(InputAction.CallbackContext context)
    {
        if (!playerState.InputEnabled)
        {
            return;
        }
        
        switch (context.action.name)
        {
            case "move":
                OnMove(context);
                break;
            case "pickup":
                OnPickup(context);
                break;
            case "throw":
                OnThrow(context);
                break;
        }
    }

    private void FixedUpdate()
    {
        if (playerState.CanWalk)
        {
            rigidbody.velocity += moveDir * (Time.fixedDeltaTime * speedMultiplier);
            playerState.WalkDirection = moveDir;

            if (Mathf.Abs(moveDir.x) > 0)
            {
                playerState.PlayerOrientation = (int)Mathf.Sign(moveDir.x);
            }
        }
        else
        {
            rigidbody.velocity = Vector2.zero;
            playerState.WalkDirection = Vector2.zero;
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
    }

    private void OnPickup(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
            {
                if (playerState.CanPickUp && pickupTargetSensor.HasPickupTarget)
                {
                    playerState.CurrentAction = PlayerAction.PickingUp;
                    pullingLoop = soundManager.PlaySfxLoop(SoundManager.Sfx.Pulling);
                }

                break;
            }
            case InputActionPhase.Canceled:
            {
                if (playerState.CurrentAction == PlayerAction.PickingUp)
                {
                    playerState.CurrentAction = PlayerAction.None;
                    soundManager.StopSfxLoop(pullingLoop);
                    pullingLoop = null;
                }

                break;
            }
            case InputActionPhase.Performed:
            {
                if (!playerState.CanPickUp || !pickupTargetSensor.HasPickupTarget)
                {
                    return;
                }

                playerState.CurrentAction = PlayerAction.Carrying;

                // TODO: Do not parent to player
                var target = pickupTargetSensor.CurrentPickupTarget.transform;
                target.SetParent(transform);
                target.gameObject.SetActive(false);
                target.localPosition = new Vector2(0, 0.2f);
                playerState.ObjectCarrying = target;
                soundManager.StopSfxLoop(pullingLoop);
                pullingLoop = null;
                soundManager.PlaySfx(SoundManager.Sfx.Pulled);
                break;
            }
        }
    }

    private void OnThrow(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Started:
            {
                if (playerState.CurrentAction != PlayerAction.Carrying || playerState.ObjectCarrying == null) return;


                playerState.CurrentAction = PlayerAction.Throwing;

                chargeThrowRoutine = StartCoroutine(ChargeThrow());
                chargingLoop = soundManager.PlaySfxLoop(SoundManager.Sfx.Charging);
                break;
            }
            case InputActionPhase.Performed:
            {
                if (playerState.CurrentAction != PlayerAction.Throwing || playerState.ObjectCarrying == null) return;

                if (chargeThrowRoutine != null)
                {
                    StopCoroutine(chargeThrowRoutine);
                }


                playerState.ObjectCarrying.SetParent(null);
                playerState.ObjectCarrying.gameObject.SetActive(true);

                var projectile = playerState.ObjectCarrying.GetComponent<ProjectileStateController>();
                var direction = playerState.PlayerOrientation;
                projectile.FireProjectile(currentThrowCharge * maxThrowDistance, direction);

                playerState.ObjectCarrying = null;
                playerState.CurrentAction = PlayerAction.None;
                soundManager.StopSfxLoop(chargingLoop);
                chargingLoop = null;
                soundManager.PlaySfx(SoundManager.Sfx.Throw);
                break;
            }
        }
    }

    private IEnumerator ChargeThrow()
    {
        var totalChargeDuration = 0f;
        currentThrowCharge = 0f;

        while (true)
        {
            yield return null;

            totalChargeDuration += Time.deltaTime;
            currentThrowCharge = Mathf.PingPong(totalChargeDuration, durationToReachMaxDistance);
        }
    }
}