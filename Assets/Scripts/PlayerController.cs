using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Animations")] [SerializeField]
    private AnimationCurve horizontalJumpAnimationCurve;

    [SerializeField] private AnimationCurve verticalUpwardsJumpAnimationCurve;
    [SerializeField] private AnimationCurve verticalDownwardsJumpAnimationCurve;

    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PickupTargetSensor pickupTargetSensor;
    [SerializeField] private new Rigidbody2D rigidbody;
    [SerializeField] private float speedMultiplier = 1f;
    [SerializeField] private float maxThrowDistance = 10f;
    [SerializeField] private float durationToReachMaxDistance = 2f;
    [SerializeField] private float stunDuration = 1.0f;

    private BeatManager beatManager;
    private Vector2? moveDir = Vector2.zero;
    private float moveRecordTime = 0;
    private IEnumerator currentMoveRoutine;

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
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();

        beatManager.OnBeatUpdate -= MoveToTheBeat;
        beatManager.OnBeatUpdate += MoveToTheBeat;
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
                OnMoveRegistered(context);
                break;
        }
    }

    private void MoveToTheBeat()
    {
        float beatTime = Time.time;
        if (playerState.CanWalk && moveDir.HasValue)
        {
            float lapsedTimeSinceBeat = beatTime - moveRecordTime;
            float beatTimingPercent = 1 - lapsedTimeSinceBeat / beatManager.Tempo;
            RestartJumpRoutine();
            //this.transform.position += (Vector3)moveDir.Value;
            playerState.WalkDirection = moveDir.Value;
            if (Mathf.Abs(moveDir.Value.x) > 0)
            {
                playerState.PlayerOrientation = (int)Mathf.Sign(moveDir.Value.x);
            }
        }
        else
        {
            rigidbody.velocity = Vector2.zero;
            playerState.WalkDirection = Vector2.zero;
        }

        moveDir = null;
        moveRecordTime = beatTime;
    }

    private void RestartJumpRoutine()
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }

        currentMoveRoutine = Move(moveDir.Value);
        StartCoroutine(currentMoveRoutine);
    }

    private IEnumerator Move(Vector2 direction)
    {
        float tempo = beatManager.Tempo * 0.2f;
        var ogPos = transform.position;
        Vector2 currentPosition = ogPos;

        for (float t = 0; t <= tempo; t += Time.deltaTime)
        {
            float lapsedPercent = Mathf.Clamp01(t / tempo);
            float y;
            if (direction.x != 0)
            {
                y = horizontalJumpAnimationCurve.Evaluate(lapsedPercent);
                currentPosition.x = ogPos.x + lapsedPercent * direction.x;
                currentPosition.y = ogPos.y + y;
            }
            else
            {
                if (direction.y > 0)
                {
                    y = verticalUpwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                    currentPosition.y = ogPos.y + y;
                }
                else if (direction.y < 0)
                {
                    y = verticalDownwardsJumpAnimationCurve.Evaluate(lapsedPercent);
                    currentPosition.y = ogPos.y - y;
                }
            }

            transform.position = currentPosition;
            yield return new WaitForEndOfFrame();
        }

        transform.position = ogPos + (Vector3)direction;
        currentMoveRoutine = null;
    }

    private void OnMoveRegistered(InputAction.CallbackContext context)
    {
        if (moveDir == null)
        {
            RecordMoveDirection(context);
        }
    }

    private void RecordMoveDirection(InputAction.CallbackContext context)
    {
        moveDir = context.ReadValue<Vector2>();
        moveRecordTime = Time.time;
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