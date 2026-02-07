using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    private PlayerState playerState;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerLocalAnimationController playerLocalAnimationController;

    private BeatManager beatManager;
    private Vector2 moveDir = Vector2.zero;

    private SoundManager soundManager;
    private float blockedUntil;

    private MapManager mapManager;

    public void Init(MapManager mapManager)
    {
        playerState = GetComponent<PlayerState>();
        this.soundManager = Services.Get<SoundManager>();
        this.beatManager = Services.Get<BeatManager>();
        this.mapManager = mapManager;
        mapManager.RegisterPlayer(this);

        if (beatManager != null)
        {
            beatManager.OnPostBeat -= CheckPlayerMoved;
            beatManager.OnPostBeat += CheckPlayerMoved;
        }
    }

    private void OnEnable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered += EventHandler;
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.onActionTriggered -= EventHandler;
    }

    private void EventHandler(InputAction.CallbackContext context)
    {
        if (!playerState.InputEnabled)
        {
            return;
        }

        if (context.action.name == "move" && context.phase == InputActionPhase.Started)
        {
            AttemptMove(context.ReadValue<Vector2>());
        }
    }

    private void RestartRoutine(IEnumerator routine)
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }

        currentMoveRoutine = routine;
        StartCoroutine(currentMoveRoutine);
    }

    public void Kill()
    {
        playerState.CurrentStateEnum = PlayerStateEnum.Dead;

        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }
    }
}
