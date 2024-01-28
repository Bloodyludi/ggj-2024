using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameObject playerPrefab;

    private PlayerState player1State;
    private PlayerState player2State;

    private void Start()
    {
        var p1 = PlayerInput.Instantiate(playerPrefab, playerIndex: 1, controlScheme: "Keyboard Left", pairWithDevice: Keyboard.current);
        var p1Transform = p1.transform;
        p1Transform.name = "Player 1";
        p1Transform.parent = mapManager.dancefloor;
        p1Transform.position = mapManager.MapToWorld(3, 3);
        player1State = p1.GetComponent<PlayerState>();
        player1State.PlayerOrientation = 1;

        var p2 = PlayerInput.Instantiate(playerPrefab, playerIndex: 2, controlScheme: "Keyboard Right", pairWithDevice: Keyboard.current);
        p2.transform.parent = mapManager.dancefloor;
        player2State = p2.GetComponent<PlayerState>();
        player2State.PlayerOrientation = -1;
        p2.transform.position = mapManager.MapToWorld(4,4);
        p2.transform.name = "Player 2";
        player2State.IsPlayer2 = true;
    }

    public void EnablePlayerInput(bool enable)
    {
        player1State.InputEnabled = enable;
        player2State.InputEnabled = enable;
    }
}