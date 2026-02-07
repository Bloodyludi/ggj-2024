using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private MapManager mapManager;
    [SerializeField] private GameObject playerPrefab;

    public PlayerState player1State;
    public PlayerState player2State;

    private void Start()
    {
        // --- Player 1 ---
        GameObject p1Obj = Instantiate(playerPrefab);
        p1Obj.name = "Player 1";
        p1Obj.transform.parent = mapManager.dancefloor;
        p1Obj.transform.position = mapManager.MapToWorld(3.6f, 3.5f);
        
        player1State = p1Obj.GetComponent<PlayerState>();
        player1State.PlayerIndex = 1; // Assign ID
        player1State.IsPlayer2 = false;
        player1State.PlayerOrientation = 1;

        // --- Player 2 ---
        GameObject p2Obj = Instantiate(playerPrefab);
        p2Obj.name = "Player 2";
        p2Obj.transform.parent = mapManager.dancefloor;
        p2Obj.transform.position = mapManager.MapToWorld(3.6f, 11.5f);

        player2State = p2Obj.GetComponent<PlayerState>();
        player2State.PlayerIndex = 2; // Assign ID
        player2State.IsPlayer2 = true;
        player2State.PlayerOrientation = -1;
    }

    public void EnablePlayerInput(bool enable)
    {
        if(player1State) player1State.InputEnabled = enable;
        if(player2State) player2State.InputEnabled = enable;
    }
}