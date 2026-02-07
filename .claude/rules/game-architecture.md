# LAB RATS - Game Architecture Deep Reference

## System Dependency Graph

```
BeatManager (central clock)
  ├── OnBeat ──────> ShakeOnBeat (camera shake)
  ├── OnBeat ──────> DancefloorTile.StartPulsing() (visual pulse)
  ├── OnPreBeat ───> (unused currently, available for pre-beat prep)
  ├── OnPostBeat ──> PlayerController.CheckPlayerMoved() (stub; miss logic now in AttemptMove)
  └── OnPostBeat ──> MapManager.UpdateDeadlyTiles() (hazard movement + death check)

GameController (orchestrator)
  ├── Awake() ─────────> Services.Register(this)
  ├── Start() ─────────> Services.Get<*>() for all managers
  ├── Start() ─────────> SoundManager.Init() (load song data)
  ├── Start() ─────────> MapManager.SetDeadlyTileSpawns() (bridge song→map)
  ├── StartMatch() ────> WaitUntil players spawned, then SoundManager.PlayMusic() + BeatManager.ShouldPerformTicks
  ├── AllPlayersDead() ─> 1.5s dramatic pause, snap timer to 0, then EvaluateGameOver
  ├── PauseGame() ─────> Time.timeScale toggle
  └── GameOver() ──────> GameOverScreen (result display)

PlayerManager (bootstrap)
  ├── Awake() ─────────> Services.Register(this)
  └── Start() ─────────> Services.Get<*>(), instantiates players via data-driven spawn configs, calls Init()

SoundManager (audio)
  ├── Awake() ─────> Services.Register(this)
  ├── Init() ──────> Services.Get<BeatManager>(), sets up SFX map and song
  ├── MusicSource ──> AudioSource (drives BeatManager timing)
  ├── sfxSource ───> AudioSource (separate channel for SFX)
  ├── CurrentSong ─> SongLevelData ScriptableObject
  └── PlaySfx() ───> One-shot sound effects with random clip selection
```

## Reference Resolution Pattern

Inter-object references use one of three patterns:
1. **`Services.Get<T>()`** for manager-to-manager references (scene singletons like BeatManager, SoundManager, MapManager, GameController, PlayerManager)
2. **`[SerializeField]`** for asset references (prefabs, AudioClips, ScriptableObjects, UI elements, Transforms)
3. **`Init()` method** for runtime-instantiated objects (called by the instantiator, e.g. PlayerController, DancefloorTile)

Managers self-register in `Awake()` via `Services.Register(this)`. The registry clears on domain reload via `[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]`.
No `FindObjectOfType`, `Find`, or `FindWithTag` calls remain.

## Event Subscription Pattern

All event subscriptions use the `OnEnable()`/`OnDisable()` lifecycle pattern:
```csharp
private void OnEnable() { beatManager.OnBeat += Handler; }
private void OnDisable() { beatManager.OnBeat -= Handler; }
```
Exception: `PlayerController.Stun.cs` dynamically subscribes/unsubscribes `ResolvePlayerStun` during stun resolution.

## Beat System (BeatManager.cs)

The heartbeat of the game. Everything gameplay-related is driven by beat events.

- **Timing source**: `GetCurrentTime()` reads `AudioSource.timeSamples / clip.frequency` (falls back to `Time.time` if audio not ready)
- **Beat position**: `GetCurrentBeatPosition()` returns `timeSamples / (frequency * beatInterval)`
- **Beat calculation**: `currentBeatPosition` floor compared to `BeatCounter` to detect new beats
- **Three-phase event cycle per beat**:
  1. `OnBeat` - exact beat moment (visual effects, camera shake)
  2. `OnPreBeat` - near end of beat (preparation window)
  3. `OnPostBeat` - start of next beat (validation: did player move? tile updates, death checks)
- **Move window**: `beatInterval * MoveWindowTimePercent` (default 10%) on each side of beat
- **BPM**: Set from SongLevelData at runtime
- **Null safety**: Update() guards against null gameController, null soundManager, and checks `MusicSource.isPlaying` before processing beats
- **Dependencies**: SoundManager and GameController resolved via `Services.Get<T>()` in `Start()`

## Song Data (SongLevelData.cs - ScriptableObject)

Centralizes per-song configuration that was previously scattered:
- `musicClip` - the AudioClip to play
- `bpm` - beats per minute (default 120)
- `startDelay` - delay before music starts
- `deadlyTileSpawns` - array of TileSpawnConfig (timing, position, direction)

Created via Assets > Create > Lab Rats > Song Level Data. Referenced by SoundManager.

## Player System (Partial Class: 3 files)

### PlayerController.cs (core)
- Holds references: PlayerState, PlayerInput, BeatManager, MapManager, SoundManager
- `Init(MapManager)` called by PlayerManager after instantiation; resolves SoundManager and BeatManager via `Services.Get<T>()`
- Subscribes to `PlayerInput.onActionTriggered` in `OnEnable()` → `EventHandler()` routes "move" actions to `AttemptMove()`
- Subscribes to `BeatManager.OnPostBeat` for `CheckPlayerMoved` in `Init()` (uses `-= +=` for safe subscription)
- `blockedUntil` timestamp prevents rapid input during animations

### PlayerController.Movement.cs
- `AttemptMove(Vector2)`: Entry point when player presses direction. Checks beat window, handles miss (combo reset + 0.1s lockout on off-beat press)
- Beat window check: `lapsedTimeSinceBeat <= moveWindow` OR `timeUntilNextBeat <= moveWindow` (stored as `hitBeat` bool)
- `MoveOnBeat()`: Increments combo, triggers animation, starts Move coroutine
- `Move()`: Coroutine using `CoroutineUtils.PacedForLoop()` that lerps position over `beatInterval * 0.2f`
- Board wrapping applied via `mapManager.GetLoopPosition()` during and after movement
- `CheckPlayerMoved()`: Stub subscribed to OnPostBeat (miss logic now handled in `AttemptMove`)

### PlayerController.Stun.cs
- `StunPlayer(direction)`: Sets Stun state, records recovery beat number
- `SetPlayerFighting(position)`: Sets Brawl state, positions at collision point
- `ResolvePlayerStun()`: Checked each beat, clears stun after `beatStunDuration` beats (default 1)

### PlayerState.cs
- States: `None` (idle) -> `MissedBeat` | `Brawl` -> `Stun` -> `Dead`
- `PlayerIndex`: identifies P1 (1) vs P2 (2), set by PlayerManager at spawn
- `CanWalk`: true only when `InputEnabled && state allows it`
- `ComboCounter`: tracked per player, displayed via ComboCounter.cs (null-guarded)
- Events: `OnStateChanged`, `OnOrientationChanged`

## Map System (Partial Class: 2 files)

### MapManager.cs (core)
- **Grid**: 5 columns x 8 rows of DancefloorTile
- **Coordinate system**: `Vector2Int` grid coords, converted via `MapToWorld()`/`WorldToMap()`
- **Wrapping**: `GetLoopPosition()` wraps coordinates modulo grid size
- **Player tracking**: Reusable `occupancyCache` dictionary maps tile positions to occupying players (GC-free)
- **Collision**: `ResolveBoardCollisions()` detects same-tile occupancy, stuns both players with Fisher-Yates shuffled directions
- **Hustle clouds**: Particle effect spawned at collision point
- **Tile spawns**: `SetDeadlyTileSpawns()` receives config from GameController (sourced from SongLevelData)
- **Dependencies**: BeatManager resolved via `Services.Get<BeatManager>()` in `Awake()`; subscribes in `OnEnable()`/`OnDisable()`
- **Tile init**: Calls `tile.Init(beatManager)` during grid creation in `Awake()`

### MapManager.Tiles.cs (hazards)
- **TileSpawnConfig**: defines when and where deadly tiles appear (from SongLevelData)
- **Flow per PostBeat**: `MoveDeadlyTiles()` -> `ResolvePlayerDeaths()` -> `SpawnNewTiles()`
- **Deadly tile movement**: Uses `HashSet<DancefloorTile>` cache to track already-updated tiles (O(1) lookup)
- **Death check**: Uses `HashSet<Vector2Int>` cache for deadly positions, iterates players backwards for safe removal
- **Spawn check**: Manual for-loop over spawn configs (no LINQ)

### DancefloorTile.cs
- Three visual states: dark (even position), bright (odd position), deadly (red)
- `Init(BeatManager)` called by MapManager during grid creation
- `StartPulsing()`: Coroutine synced to `BeatManager.NextBeatTime` using `beatManager.GetCurrentTime()` (null-guarded)
- Beat-synced scale pulse with random jitter for visual variety

## Shared Utility (CoroutineUtils.cs)

Static class providing `PacedForLoop(float duration, Action<float>)`:
- Iterates over duration, calling callback with normalized progress [0..1]
- Uses `yield return null` (WebGL-safe, zero allocation per frame)
- Replaces duplicate implementations that previously existed in PlayerController.Movement and PlayerLocalAnimationController

## Animation System

### PlayerAnimationController.cs
- Drives Animator state machine based on PlayerState changes
- Separate Animator controllers for Player 1 and Player 2 (different sprites)
- Orientation handled via `localScale.x` flip (-1 or 1)
- Animation params: Walking direction vector mapped to Up/Down/Left/Right states

### PlayerLocalAnimationController.cs
- Handles local sprite offset animations (jump, bob up/down)
- Uses `CoroutineUtils.PacedForLoop()` with AnimationCurves for easing
- Applied to sprite child transform's localPosition

## UI Flow

```
MainMenu (MainMenuController)
  ├── Start Game ──> SampleScene
  ├── How To ──────> HowTo scene (HowToController)
  ├── Credits ─────> Credits scene (CreditsController)
  └── Exit ────────> Application.Quit() (no-op in WebGL)

SampleScene (gameplay)
  ├── PauseScreen ──> Resume / Quit to MainMenu
  └── GameOverScreen ──> Replay (reload SampleScene) / MainMenu
```

## Win Condition Logic (GameController.cs)

```
if (all players dead) -> 1.5s dramatic pause, snap timer to 0, then evaluate:
  if (both dead) -> Lose (draw)
  if (one dead) -> survivor wins
if (time up, both alive) -> higher combo wins (or Draw)
```
Note: `StartMatch()` uses `WaitUntil(() => playerManager.PlayerStates.Count >= 2)` to ensure players are spawned before starting.

## Key Constants & Tuning Values
- Match duration: 180 seconds
- Default BPM: 120 (set from SongLevelData)
- Beat move window: 10% of beat interval
- Stun duration: 1 beat
- Grid size: 5 x 8
- Player start positions: P1 (row 3.6, col 3.5), P2 (row 3.6, col 11.5)
- Movement animation duration: 20% of beat interval

## SFX Enum (SoundManager.Sfx)
- `PlayerHit`, `BounceWater`, `BounceRat`, `BouncePlayer`
- `Pulling`, `Pulled`, `Landing`, `Charging`, `Throw`
- Inspector fields use `[FormerlySerializedAs]` to preserve data from legacy `carrot*` names
