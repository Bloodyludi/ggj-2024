using System;
using UnityEngine;

public enum PlayerStateEnum
{
    None,
    MissedBeat,
    Brawl,
    Stunned,
    Dead,
}

public class PlayerState : MonoBehaviour
{
    public bool InputEnabled { get; set; } = true;

    public event Action<PlayerStateEnum> OnStateChanged;

    private PlayerStateEnum currentState = PlayerStateEnum.None;

    public PlayerStateEnum CurrentStateEnum
    {
        get => currentState;
        set
        {
            OnStateChanged?.Invoke(value);
            currentState = value;
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

    public Vector2 WalkDirection { get; set; }
    
    public bool CanWalk => InputEnabled && CurrentStateEnum != PlayerStateEnum.Stunned && CurrentStateEnum != PlayerStateEnum.Dead;
    public bool IsPlayer2 { get; set; }
}
