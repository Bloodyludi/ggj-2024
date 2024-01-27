using System;
using UnityEngine;

public enum PlayerAction
{
    None,
    PickingUp,
    Carrying,
    Throwing,
    Stunned,
}

public class PlayerState : MonoBehaviour
{
    public bool InputEnabled { get; set; } = true;

    public event Action<PlayerAction> OnActionChanged;

    private PlayerAction currentAction = PlayerAction.None;

    public PlayerAction CurrentAction
    {
        get => currentAction;
        set
        {
            OnActionChanged?.Invoke(value);
            currentAction = value;
        }
    }
    public event Action<int> OnOrientationChanged;

    private int playerOrientation = 1;

    public int PlayerOrientation
    {
        get => playerOrientation;
        set
        {
            if (playerOrientation != value)
            {
                playerOrientation = value;
                OnOrientationChanged?.Invoke(value);
            }
        }
    }

    public bool IsWalking => WalkDirection.magnitude > 0f;
    public Vector2 WalkDirection { get; set; }
    public Transform ObjectCarrying { get; set; }
    
    public bool CanWalk => InputEnabled && CurrentAction != PlayerAction.Stunned && CurrentAction != PlayerAction.PickingUp && CurrentAction != PlayerAction.Throwing;
    public bool CanPickUp => InputEnabled && CurrentAction != PlayerAction.Stunned && CurrentAction != PlayerAction.Carrying && CurrentAction != PlayerAction.Throwing;
    public bool IsPlayer2 { get; set; }
}
