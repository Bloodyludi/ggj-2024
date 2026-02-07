# LAB RATS - Project Context for Claude

## Maintenance Rule
This is a **living document**. When you make changes that affect the project's architecture, file structure, systems, conventions, or constraints described here, you MUST update this file (and any relevant `.claude/rules/*.md` files) as part of the same task. Examples: adding/removing/renaming scripts, introducing new systems or design patterns, changing build targets, modifying the input scheme, or altering game mechanics. Keeping this file accurate is critical - future sessions depend on it.

## Overview
2-player local multiplayer rhythm game (Global Game Jam 2024). Players control lab rats that must dance to the beat of the music while dodging hazardous tiles on a 5x8 dancefloor grid. Winner is the last rat alive or the one with the highest combo when time runs out.

## Tech Stack
- **Engine**: Unity (2D, URP not used - built-in render pipeline)
- **Language**: C# (.NET Standard)
- **Input**: Unity Input System package (1.17.0) - dual keyboard scheme (WASD + Arrow keys)
- **Audio**: Unity AudioSource with `Time.timeAsDouble` beat synchronization (WebGL-safe). Separate AudioSources for music and SFX.
- **Build Target**: WebGL only (live at https://www.llauer.de/ggj-2024/)
- **UI**: Unity UI (uGUI) + TextMeshPro
- **Post-Processing**: Unity Post Processing Stack v2

## Project Structure
```
Assets/
  Scripts/           # All C# game scripts (flat structure, plus Utils/ subfolder)
    Utils/           # Vector2Extensions.cs
    CoroutineUtils.cs      # Shared PacedForLoop utility (WebGL-safe, yield return null)
    Services.cs            # Static service locator for manager cross-references
    SongLevelData.cs       # ScriptableObject for song configuration
    SongLibrary.cs         # ScriptableObject listing all available songs + static SelectedSong
    SongSelector.cs        # MainMenu UI component for cycling through songs
  SoundFX/
    SoundData/           # SongLevelData assets (Ludwig.asset, Ludwig 1.asset, Ludwig 2.asset)
  Scenes/            # SampleScene (gameplay), MainMenu, HowTo, Credits, FireInTheHole
  Prefabs/           # Player, GameController, DancefloorTile, GameUI, SoundManager
  Animators/         # Player 1 & Player 2 animator controllers + animations
  Art/               # Sprites, UI textures, particles
  SoundFX/           # Music tracks (.mp3) and sound effects
  TextMesh Pro/      # Font assets
```

## Core Systems (read .claude/rules/game-architecture.md for deep detail)

| System | Files | Role |
|--------|-------|------|
| Beat/Rhythm | `BeatManager.cs` | Central clock. Fires OnBeat/OnPreBeat/OnPostBeat events. Timing via `GetCurrentTime()` (audio sample position with `Time.time` fallback) |
| Player | `PlayerController.cs`, `.Movement.cs`, `.Stun.cs` | Partial class. Handles input, beat-window movement, stun/brawl. Injected via `Init()` method. |
| Player State | `PlayerState.cs` | State machine: None, MissedBeat, Brawl, Stun, Dead. Tracks `PlayerIndex` (1 or 2) and `ComboCounter` |
| Map | `MapManager.cs`, `MapManager.Tiles.cs` | 5x8 wrapping grid. Deadly tile spawning, movement, collision. GC-optimized with reusable caches. |
| Tiles | `DancefloorTile.cs` | Individual tile: safe/deadly state, beat-synced pulsing. Injected via `Init(BeatManager)`. |
| Game Flow | `GameController.cs` | 180s timer, pause, win condition evaluation. Bridges SongLevelData to MapManager. Dramatic 1.5s death pause when all players die. |
| Audio | `SoundManager.cs` | Music playback via SongLevelData, SFX on separate AudioSource with random clip selection |
| Song Data | `SongLevelData.cs` | ScriptableObject: music clip, BPM, start delay, beat window override, deadly tile spawn configs |
| Song Selection | `SongLibrary.cs`, `SongSelector.cs` | ScriptableObject song list + MainMenu song picker UI. `OnSongChanged` event, static `SongLibrary.SelectedSong` passes selection across scenes. |
| Animation | `PlayerAnimationController.cs`, `PlayerLocalAnimationController.cs` | Sprite animation + local movement (jump/bob) |
| UI | `MainMenuController.cs`, `PauseScreen.cs`, `GameOverScreen.cs`, `ComboCounter.cs`, `CountdownTimer.cs` | Scene navigation, HUD |
| Service Locator | `Services.cs` | Static registry for manager cross-references. Managers self-register in `Awake()`, look up via `Services.Get<T>()` |
| Utility | `CoroutineUtils.cs`, `Vector2Extensions.cs`, `ShakeOnBeat.cs`, `AutoDestroy.cs`, `MatchWidth.cs` | Shared coroutine helpers, extensions, camera shake, cleanup, aspect ratio |

## Key Design Patterns
- **Service Locator**: `Services` static class for manager-to-manager references. Managers self-register in `Awake()` via `Services.Register(this)` and look up other managers via `Services.Get<T>()`. Falls back to `FindFirstObjectByType<T>()` if not yet registered. Clears on domain reload via `[RuntimeInitializeOnLoadMethod]`.
- **Event-driven**: BeatManager broadcasts beat events; systems subscribe via `OnEnable()`/`OnDisable()` rather than poll
- **Partial classes**: PlayerController split into 3 files, MapManager split into 2
- **Coroutine animations**: Movement and effects use `CoroutineUtils.PacedForLoop()` with AnimationCurves
- **Dependency injection**: `[SerializeField]` for asset references (prefabs, AudioClips, ScriptableObjects, UI elements, Transforms); `Services.Get<T>()` for manager cross-references; `Init()` methods for runtime-instantiated objects (PlayerController, DancefloorTile)
- **ScriptableObject configuration**: Song data (clip, BPM, delay, tile spawns) stored in `SongLevelData` assets
- **GC-optimized per-beat callbacks**: Reusable caches (dictionaries, hash sets, lists) instead of LINQ allocations
- **Data-driven player spawning**: PlayerManager uses a static spawn config array for player instantiation
- **Cross-scene song selection**: `SongLibrary.SelectedSong` static property passes the player's song choice from MainMenu to SampleScene. SoundManager reads it in `Init()`, falling back to the Inspector-wired default if null.

## Game Mechanics Quick Reference
- **Beat window**: 10% of beat interval on each side (configurable via MoveWindowTimePercent, overridable per-song via SongLevelData.moveWindowTimePercent)
- **Combo**: +1 per beat-synced move, resets to 0 on off-beat press (miss)
- **Board wrapping**: Players wrap around all edges seamlessly
- **Player collision**: Both players stunned for 1 beat, flung in random directions (Fisher-Yates shuffle)
- **Deadly tiles**: Move each beat in configured direction, instant kill on contact
- **Death pause**: When all players die, 1.5s dramatic pause before game over screen

## Hard Constraints
- NEVER edit `.meta` files - these are Unity-managed asset references
- NEVER edit files in `Library/`, `Temp/`, `obj/`, or `Logs/` - auto-generated
- NEVER edit `ProjectSettings/` files directly - use Unity Editor
- Changing `[SerializeField]` fields can break prefab/scene references - flag these changes
- All scripts live in `Assets/Scripts/` (flat, plus `Utils/` subfolder)
- Two control schemes: "Keyboard Left" (P1: WASD) and "Keyboard Right" (P2: arrows)

## WebGL Constraints (this is a WebGL-only project)
- NEVER use `AudioSettings.dspTime` - it is unreliable/unavailable in WebGL. Use `Time.timeAsDouble` instead
- NEVER use `System.Threading` or `Task`/`async`/`await` - WebGL is single-threaded with no threading support
- NEVER use `System.IO` file operations (File.Read/Write) - there is no filesystem access in WebGL
- NEVER use `System.Net.Sockets` or raw networking - use `UnityWebRequest` if HTTP is needed
- NEVER use `Application.Quit()` for actual exit - it has no effect in WebGL (OK in menu UI as a no-op)
- NEVER use `WaitForEndOfFrame` in coroutines - it can cause issues in WebGL. Use `yield return null` instead.
- Avoid large blocking operations - WebGL runs on the browser main thread and will freeze the tab
- Compressed audio formats: use `.mp3` or `.ogg` (not `.wav`) to minimize download size
- Minimize asset sizes - everything is downloaded before play
- `PlayerPrefs` works in WebGL (uses browser IndexedDB) - safe for save data if needed
- No `#if !UNITY_WEBGL` preprocessor blocks remain - WebGL is the only build target

## Code Style
- PascalCase: classes, methods, properties, public fields
- camelCase: local variables, parameters, private fields
- `[SerializeField]` for inspector-exposed private fields
- Minimal comments - code is self-documenting
- No unit tests currently exist

## Build Target
- **WebGL only** - live at https://www.llauer.de/ggj-2024/
- All code must be WebGL-compatible (see WebGL Constraints above)
