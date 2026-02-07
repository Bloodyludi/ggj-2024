# LAB RATS - Game Architecture Deep Reference

## System Dependency Graph

```
BeatManager (central clock)
  ├── OnBeat ──────> ShakeOnBeat (camera shake)
  ├── OnBeat ──────> DancefloorTile.StartPulsing() (visual pulse)
  ├── OnPreBeat ───> (unused currently, available for pre-beat prep)
  ├── OnPostBeat ──> PlayerController.CheckPlayerMoved() (combo validation)
  └── OnPostBeat ──> MapManager.UpdateDeadlyTiles() (hazard movement + death check)

GameController (orchestrator)
  ├── OnCustomUpdate ──> CountdownTimer (HUD update)
  ├── PauseGame() ─────> Time.timeScale toggle
  ├── GameOver() ──────> GameOverScreen (result display)
  └── SpeedUpTrigger ──> SoundManager (tempo change at 60s remaining)

PlayerManager (bootstrap)
  └── Instantiates Player 1 & Player 2 with separate control schemes

SoundManager (audio)
  ├── MusicSource ──> AudioSource (drives BeatManager timing)
  └── PlaySFX() ───> One-shot sound effects with random clip selection
```

## Beat System (BeatManager.cs)

The heartbeat of the game. Everything gameplay-related is driven by beat events.

- **Timing source**: `Time.timeAsDouble` for WebGL-safe high-precision timing (legacy code may reference `AudioSource.timeSamples / clip.frequency` but the WebGL path uses `Time.timeAsDouble`)
- **Beat calculation**: `currentAudioTime / beatInterval` where `beatInterval = 60f / bpm`
- **Three-phase event cycle per beat**:
  1. `OnBeat` - exact beat moment (visual effects, camera shake)
  2. `OnPreBeat` - near end of beat (preparation window)
  3. `OnPostBeat` - start of next beat (validation: did player move? tile updates, death checks)
- **Move window**: `beatInterval * MoveWindowTimePercent` (default 10%) on each side of beat
- **BPM**: Default 120, changed at runtime for speed-up

## Player System (Partial Class: 3 files)

### PlayerController.cs (core)
- Holds references: PlayerState, PlayerInput, BeatManager, MapManager, SoundManager
- Subscribes to `PlayerInput.onActionTriggered` for input
- Subscribes to `BeatManager.OnPostBeat` for move validation
- `blockedUntil` timestamp prevents rapid input during animations

### PlayerController.Movement.cs
- `OnMoveRegistered()`: Entry point when player presses direction
- Beat window check: `lapsedTimeSinceBeat <= moveWindowSeconds` OR `timeUntilNextBeat <= moveWindowSeconds`
- `MoveOnBeat()`: Increments combo, triggers animation, starts Move coroutine
- `Move()`: Coroutine that lerps position over `beatInterval * 0.2f`
- `CheckPlayerMoved()`: Called on PostBeat - resets combo if player didn't move in window

### PlayerController.Stun.cs
- `StunPlayer(direction)`: Sets Stun state, records recovery beat number
- `SetPlayerFighting(position)`: Sets Brawl state, positions at collision point
- `ResolvePlayerStun()`: Checked each beat, clears stun after `beatStunDuration` beats (default 1)

### PlayerState.cs
- States: `None` (idle) -> `MissedBeat` | `Brawl` -> `Stun` -> `Dead`
- `CanWalk`: true only when `InputEnabled && state allows it`
- `ComboCounter`: tracked per player, displayed via ComboCounter.cs
- Events: `OnStateChanged`, `OnOrientationChanged`

## Map System (Partial Class: 2 files)

### MapManager.cs (core)
- **Grid**: 5 columns x 8 rows of DancefloorTile
- **Coordinate system**: `Vector2Int` grid coords, converted via `MapToWorld()`/`WorldToMap()`
- **Wrapping**: `GetLoopPosition()` wraps coordinates modulo grid size
- **Player tracking**: Dictionary maps tile positions to occupying players
- **Collision**: `ResolveBoardCollisions()` detects same-tile occupancy, stuns both players
- **Hustle clouds**: Particle effect spawned at collision point

### MapManager.Tiles.cs (hazards)
- **TileSpawnConfig**: defines when and where deadly tiles appear (inspector-configured)
- **Flow per PostBeat**: `MoveDeadlyTiles()` -> `SpawnNewTiles()` -> `ResolvePlayerDeaths()`
- **Deadly tile movement**: Each deadly tile shifts in its `movementDirection` each beat
- **Death check**: Any player occupying a deadly tile is killed instantly

### DancefloorTile.cs
- Three visual states: dark (even position), bright (odd position), deadly (red)
- `StartPulsing()`: Coroutine synced to `BeatManager.NextBeatTime` using `Time.timeAsDouble` (WebGL-safe)
- Beat-synced scale pulse with random jitter for visual variety
- Note: Contains legacy `#if !UNITY_WEBGL` blocks - only the WebGL (`#else`) path is active

## Animation System

### PlayerAnimationController.cs
- Drives Animator state machine based on PlayerState changes
- Separate Animator controllers for Player 1 and Player 2 (different sprites)
- Orientation handled via `localScale.x` flip (-1 or 1)
- Animation params: Walking direction vector mapped to Up/Down/Left/Right states

### PlayerLocalAnimationController.cs
- Handles local sprite offset animations (jump, bob up/down)
- Uses AnimationCurves for easing
- Applied to sprite child transform's localPosition

## UI Flow

```
MainMenu (MainMenuController)
  ├── Start Game ──> SampleScene
  ├── How To ──────> HowTo scene (HowToController)
  ├── Credits ─────> Credits scene (CreditsController)
  └── Exit ────────> Application.Quit()

SampleScene (gameplay)
  ├── PauseScreen ──> Resume / Quit to MainMenu
  └── GameOverScreen ──> Replay (reload SampleScene) / MainMenu
```

## Win Condition Logic (GameController.cs)

```
if (both dead) -> player with higher combo wins (or Draw)
if (one dead) -> survivor wins
if (time up, both alive) -> higher combo wins (or Draw)
if (time up, both dead before end) -> same as both dead
```

## Key Constants & Tuning Values
- Match duration: 180 seconds
- Speed-up threshold: 60 seconds remaining
- Default BPM: 120
- Beat move window: 10% of beat interval
- Stun duration: 1 beat
- Grid size: 5 x 8
- Player start positions: P1 (3.6, 3.5), P2 (3.6, 11.5)
- Movement animation duration: 20% of beat interval
