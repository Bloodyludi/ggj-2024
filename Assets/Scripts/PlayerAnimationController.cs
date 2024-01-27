using System;
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

    private static readonly string[] Actions =
    {
        "NoneAction",
        "PickingUpAction",
        "CarryingAction",
        "ThrowingAction",
        "StunnedAction",
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
        playerState.OnActionChanged += OnPlayerActionChanged;
        playerState.OnOrientationChanged += OnPlayerOrientationChanged;

        OnPlayerActionChanged(playerState.CurrentAction);
        OnPlayerOrientationChanged(playerState.PlayerOrientation);
    }

    private void OnDisable()
    {
        playerState.OnActionChanged -= OnPlayerActionChanged;
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
        animator.SetBool(Idle, !playerState.IsWalking);
        animator.SetBool(Up, playerState.WalkDirection.y > 0);
        animator.SetBool(Down, playerState.WalkDirection.y < 0);
        animator.SetBool(Left, playerState.WalkDirection.x > 0);
        animator.SetBool(Right, playerState.WalkDirection.x < 0);
    }

    private void OnPlayerActionChanged(PlayerAction action)
    {
        var currentActionStr = $"{action.ToString()}Action";
        foreach (var actionStr in Actions)
        {
            animator.SetBool(actionStr,currentActionStr.Equals(actionStr));
        }
    }
}
