using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private MapManager mapManager;

    public List<PlayerState> PlayerStates { get; } = new();

    private struct PlayerSpawnConfig
    {
        public int playerIndex;
        public float spawnRow;
        public float spawnColumn;
        public int orientation;
        public bool isPlayer2;
    }

    private static readonly PlayerSpawnConfig[] spawnConfigs =
    {
        new() { playerIndex = 1, spawnRow = 3.6f, spawnColumn = 3.5f, orientation = 1, isPlayer2 = false },
        new() { playerIndex = 2, spawnRow = 3.6f, spawnColumn = 11.5f, orientation = -1, isPlayer2 = true },
    };

    private void Awake()
    {
        Services.Register(this);
    }

    private void Start()
    {
        mapManager = Services.Get<MapManager>();

        foreach (var config in spawnConfigs)
        {
            var obj = Instantiate(playerPrefab);
            var t = obj.transform;
            t.name = $"Player {config.playerIndex}";
            t.parent = mapManager.dancefloor;
            t.position = mapManager.MapToWorld(config.spawnRow, config.spawnColumn);

            var state = obj.GetComponent<PlayerState>();
            state.PlayerIndex = config.playerIndex;
            state.PlayerOrientation = config.orientation;
            state.IsPlayer2 = config.isPlayer2;
            PlayerStates.Add(state);

            obj.GetComponent<PlayerController>().Init(mapManager);
        }
    }

    public void EnablePlayerInput(bool enable)
    {
        foreach (var state in PlayerStates)
        {
            if (state) state.InputEnabled = enable;
        }
    }
}
