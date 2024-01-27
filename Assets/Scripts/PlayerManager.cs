using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform p1Spawn;
    [SerializeField] private Transform p2Spawn;

    private PlayerState player1State;
    private PlayerState player2State;

    private void Start()
    {
        var p1 = PlayerInput.Instantiate(playerPrefab, playerIndex: 1, controlScheme: "Keyboard Left", pairWithDevice: Keyboard.current);
        var p1Transform = p1.transform;
        p1Transform.position = p1Spawn.position;
        player1State = p1.GetComponent<PlayerState>();
        player1State.PlayerOrientation = 1;

        var p2 = PlayerInput.Instantiate(playerPrefab, playerIndex: 2, controlScheme: "Keyboard Right", pairWithDevice: Keyboard.current);
        var p2Transform = p2.transform;
        p2Transform.position = p2Spawn.position;
        player2State = p2.GetComponent<PlayerState>();
        player2State.PlayerOrientation = -1;
        player2State.IsPlayer2 = true;
    }

    public void EnablePlayerInput(bool enable)
    {
        player1State.InputEnabled = enable;
        player2State.InputEnabled = enable;
    }
}