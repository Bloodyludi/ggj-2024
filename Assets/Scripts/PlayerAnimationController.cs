using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private bool isPlayerTwo;
    [SerializeField] private PlayerState playerState;
    [SerializeField] private Animator animator;

    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Up = Animator.StringToHash("Up");
    private static readonly int Down = Animator.StringToHash("Down");
    private static readonly int Left = Animator.StringToHash("Left");
    private static readonly int Right = Animator.StringToHash("Right");

    private static readonly string[] States =
    {
        "Idle",
        "MissedBeat",
        "Brawl",
        "Stun",
        "Dead",
    };

    private void Start()
    {
        if (isPlayerTwo)
        {
            gameObject.SetActive(playerState.IsPlayer2);
        }
        else
        {
            gameObject.SetActive(!playerState.IsPlayer2);
        }
    }

    private void OnEnable()
    {
        playerState.OnStateChanged += OnPlayerStateChanged;
        playerState.OnOrientationChanged += OnPlayerOrientationChanged;

        OnPlayerStateChanged(playerState.CurrentStateEnum);
        OnPlayerOrientationChanged(playerState.PlayerOrientation);
    }

    private void OnDisable()
    {
        playerState.OnStateChanged -= OnPlayerStateChanged;
        playerState.OnOrientationChanged -= OnPlayerOrientationChanged;
    }

    private void OnPlayerOrientationChanged(int dir)
    {
        var scale = transform.localScale;
        scale = new Vector3(Mathf.Abs(scale.x) * dir, scale.y, scale.z);
        transform.localScale = scale;
    }

    private void LateUpdate()
    {
        UpdateWalking();
    }

    private void UpdateWalking()
    {
        if (playerState.CurrentStateEnum != PlayerStateEnum.None)
        {
            return;
        }

        animator.SetBool(Idle, playerState.WalkDirection.magnitude <= 0f);
        animator.SetBool(Up, playerState.WalkDirection.y > 0);
        animator.SetBool(Down, playerState.WalkDirection.y < 0);
        animator.SetBool(Left, playerState.WalkDirection.x > 0);
        animator.SetBool(Right, playerState.WalkDirection.x < 0);
    }

    private void OnPlayerStateChanged(PlayerStateEnum stateEnum)
    {
        var currentState = stateEnum.ToString();
        foreach (var state in States)
        {
            animator.SetBool(state, currentState.Equals(state));
        }

        animator.SetBool(Idle, false);
        animator.SetBool(Up, false);
        animator.SetBool(Down, false);
        animator.SetBool(Left, false);
        animator.SetBool(Right, false);
    }
}