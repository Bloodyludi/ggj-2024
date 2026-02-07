using System.Collections;
using UnityEngine;

public partial class PlayerController : MonoBehaviour
{
    private PlayerState playerState;
    [SerializeField] private PlayerLocalAnimationController playerLocalAnimationController;
    
    private BeatManager beatManager; 
    private Vector2 moveDir = Vector2.zero;
    
    private SoundManager soundManager;
    private float blockedUntil;
    
    private MapManager mapManager;

    private void Awake()
    {
        playerState = GetComponent<PlayerState>();

        var soundObj = GameObject.FindWithTag("Sound");
        if(soundObj) soundManager = soundObj.GetComponent<SoundManager>();

        // Fix: Use FindFirstObjectByType (New API)
        beatManager = FindFirstObjectByType<BeatManager>();
        
        // This find is string based, so it's fine, but MapManager might need update
        mapManager = GameObject.Find("Map").GetComponent<MapManager>();
        mapManager.RegisterPlayer(this);

        if (beatManager != null)
        {
            beatManager.OnPostBeat -= CheckPlayerMoved;
            beatManager.OnPostBeat += CheckPlayerMoved;
        }
    }

    private void Update()
    {
        if (!playerState.InputEnabled || beatManager == null) return;

        Vector2 inputDir = Vector2.zero;

        // Player 1 Controls (WASD)
        if (playerState.PlayerIndex == 1)
        {
            if (Input.GetKeyDown(KeyCode.W)) inputDir = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.S)) inputDir = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.A)) inputDir = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.D)) inputDir = Vector2.right;
        }
        // Player 2 Controls (Arrows)
        else if (playerState.PlayerIndex == 2)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow)) inputDir = Vector2.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow)) inputDir = Vector2.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow)) inputDir = Vector2.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow)) inputDir = Vector2.right;
        }

        if (inputDir != Vector2.zero)
        {
            AttemptMove(inputDir);
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