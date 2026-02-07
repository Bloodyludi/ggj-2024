using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private PlayerLocalAnimationController playerLocalAnimationController;
    private BeatManager beatManager;
    private Vector2 moveDir = Vector2.zero;

    private SoundManager soundManager;
    private float blockedUntil;

    private MapManager mapManager;

    public void Init(MapManager mapManager)
    {
        this.soundManager = Services.Get<SoundManager>();
        this.beatManager = Services.Get<BeatManager>();
        this.mapManager = mapManager;
        mapManager.RegisterPlayer(this);
    }

    private void OnEnable()
    {
        playerInput.onActionTriggered += EventHandler;
        if (beatManager != null)
        {
            beatManager.OnPostBeat += CheckPlayerMoved;
        }
    }

    private void OnDisable()
    {
        playerInput.onActionTriggered -= EventHandler;
        if (beatManager != null)
        {
            beatManager.OnPostBeat -= CheckPlayerMoved;
        }
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
    private void RestartRoutine(IEnumerator routine)
    {
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }

        currentMoveRoutine =routine;
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
