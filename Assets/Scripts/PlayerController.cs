using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerState playerState;
    [SerializeField] private PlayerInput playerInput;
    
    private BeatManager beatManager;
    private Vector2 moveDir = Vector2.zero;
    
    private SoundManager soundManager;
    private float blockedTime;
    
    private MapManager mapManager;

    private void Awake()
    {
        soundManager = GameObject.FindWithTag("Sound")?.GetComponent<SoundManager>();
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();
        mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        mapManager.RegisterPlayer(this);

        beatManager.OnPostBeat -= CheckPlayerMoved;
        beatManager.OnPostBeat += CheckPlayerMoved;
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
        Debug.Log($"Player died: {transform.name} pos: {mapManager.WorldToMap(transform.position)}");
        
        playerState.CurrentStateEnum = PlayerStateEnum.Dead;
        
        if (currentMoveRoutine != null)
        {
            StopCoroutine(currentMoveRoutine);
        }
    }
}