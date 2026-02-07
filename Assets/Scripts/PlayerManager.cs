using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private GameObject playerPrefab;

    private MapManager mapManager;
    private SoundManager soundManager;
    private BeatManager beatManager;

    public List<PlayerState> PlayerStates { get; } = new();

    private struct PlayerSpawnConfig
    {
        public int playerIndex;
        public string controlScheme;
        public float spawnRow;
        public float spawnColumn;
        public int orientation;
        public bool isPlayer2;
    }

    private static readonly PlayerSpawnConfig[] spawnConfigs =
    {
        new() { playerIndex = 1, controlScheme = "Keyboard Left", spawnRow = 3.6f, spawnColumn = 3.5f, orientation = 1, isPlayer2 = false },
        new() { playerIndex = 2, controlScheme = "Keyboard Right", spawnRow = 3.6f, spawnColumn = 11.5f, orientation = -1, isPlayer2 = true },
    };

    private void Awake()
    {
        Services.Register(this);
    }

    private void Start()
    {
        mapManager = Services.Get<MapManager>();
        soundManager = Services.Get<SoundManager>();
        beatManager = Services.Get<BeatManager>();

        foreach (var config in spawnConfigs)
        {
            var playerInput = PlayerInput.Instantiate(playerPrefab, playerIndex: config.playerIndex, controlScheme: config.controlScheme, pairWithDevice: Keyboard.current);
            var t = playerInput.transform;
            t.name = $"Player {config.playerIndex}";
            t.parent = mapManager.dancefloor;
            t.position = mapManager.MapToWorld(config.spawnRow, config.spawnColumn);

            var state = playerInput.GetComponent<PlayerState>();
            state.PlayerOrientation = config.orientation;
            state.IsPlayer2 = config.isPlayer2;
            PlayerStates.Add(state);

            playerInput.GetComponent<PlayerController>().Init(mapManager);
        }
    }

    public void EnablePlayerInput(bool enable)
    {
        foreach (var state in PlayerStates)
        {
            state.InputEnabled = enable;
        }
    }
}
