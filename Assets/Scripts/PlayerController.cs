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

    private void Awake()
    {
        soundManager = GameObject.FindWithTag("Sound")?.GetComponent<SoundManager>();
        beatManager = GameObject.Find("BeatManager").GetComponent<BeatManager>();
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
}