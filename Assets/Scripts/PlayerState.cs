using System;
using UnityEngine;

public enum PlayerStateEnum
{
    None,
    MissedBeat,
    Brawl,
    Stun,
    Dead,
}

public class PlayerState : MonoBehaviour
{
    [SerializeField] private ComboCounter comboCounterLabel;
    
    // --- ADDED: Required for PlayerController to identify P1 vs P2 ---
    public int PlayerIndex = 1; 

    public int ComboCounter
    {
        get => comboCounter;
        set
        {
            comboCounter = value;
            if(comboCounterLabel != null)
                comboCounterLabel.UpdateComboCount(comboCounter);

            if (CurrentStateEnum == PlayerStateEnum.Dead ||
                CurrentStateEnum == PlayerStateEnum.Brawl ||
                CurrentStateEnum == PlayerStateEnum.Stun)
            {
                return;
            }
            
            if (comboCounter == 0)
            {
                CurrentStateEnum = PlayerStateEnum.MissedBeat;
            }
            else
            {
                CurrentStateEnum = PlayerStateEnum.None;
            }
        }
    }

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
    private int comboCounter;

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

    public bool CanWalk => InputEnabled &&
                           CurrentStateEnum != PlayerStateEnum.Stun
                           && CurrentStateEnum != PlayerStateEnum.Brawl
                           && CurrentStateEnum != PlayerStateEnum.Dead;

    public bool IsPlayer2 { get; set; }
}