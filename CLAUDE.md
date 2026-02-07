# LAB RATS - Project Context for Claude

## Maintenance Rule
This is a **living document**. When you make changes that affect the project's architecture, file structure, systems, conventions, or constraints described here, you MUST update this file (and any relevant `.claude/rules/*.md` files) as part of the same task. Examples: adding/removing/renaming scripts, introducing new systems or design patterns, changing build targets, modifying the input scheme, or altering game mechanics. Keeping this file accurate is critical - future sessions depend on it.

## Overview
2-player local multiplayer rhythm game (Global Game Jam 2024). Players control lab rats that must dance to the beat of the music while dodging hazardous tiles on a 5x8 dancefloor grid. Winner is the last rat alive or the one with the highest combo when time runs out.

## Tech Stack
- **Engine**: Unity (2D, URP not used - built-in render pipeline)
- **Language**: C# (.NET Standard)
- **Input**: Unity Input System package (1.17.0) - dual keyboard scheme (WASD + Arrow keys)
- **Audio**: Unity AudioSource with `Time.timeAsDouble` beat synchronization (WebGL-safe)
- **Build Target**: WebGL only (live at https://www.llauer.de/ggj-2024/)
- **UI**: Unity UI (uGUI) + TextMeshPro
- **Post-Processing**: Unity Post Processing Stack v2

## Project Structure
```
Assets/
  Scripts/           # All 24 C# game scripts (flat structure)
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
| Beat/Rhythm | `BeatManager.cs` | Central clock. Fires OnBeat/OnPreBeat/OnPostBeat events synced via `Time.timeAsDouble` |
| Player | `PlayerController.cs`, `.Movement.cs`, `.Stun.cs` | Partial class. Handles input, beat-window movement, stun/brawl |
| Player State | `PlayerState.cs` | State machine: None, MissedBeat, Brawl, Stun, Dead |
| Map | `MapManager.cs`, `MapManager.Tiles.cs` | 5x8 wrapping grid. Deadly tile spawning, movement, collision |
| Tiles | `DancefloorTile.cs` | Individual tile: safe/deadly state, beat-synced pulsing |
| Game Flow | `GameController.cs` | 180s timer, pause, speed-up at 60s, win condition evaluation |
| Audio | `SoundManager.cs` | Music playback with BPM sync, SFX with random clip selection |
| Animation | `PlayerAnimationController.cs`, `PlayerLocalAnimationController.cs` | Sprite animation + local movement (jump/bob) |
| UI | `MainMenuController.cs`, `PauseScreen.cs`, `GameOverScreen.cs`, `ComboCounter.cs`, `CountdownTimer.cs` | Scene navigation, HUD |
| Utility | `Vector2Extensions.cs`, `ShakeOnBeat.cs`, `AutoDestroy.cs`, `MatchWidth.cs` | Extensions, camera shake, cleanup, aspect ratio |

## Key Design Patterns
- **Event-driven**: BeatManager broadcasts beat events; systems subscribe rather than poll
- **Partial classes**: PlayerController split into 3 files, MapManager split into 2
- **Coroutine animations**: Movement and effects use `PacedForLoop()` with AnimationCurves
- **Component-based**: MonoBehaviours with loose coupling via `FindObjectOfType`

## Game Mechanics Quick Reference
- **Beat window**: 10% of beat interval on each side (configurable via MoveWindowTimePercent)
- **Combo**: +1 per beat-synced move, resets to 0 on miss
- **Board wrapping**: Players wrap around all edges seamlessly
- **Player collision**: Both players stunned for 1 beat, flung in random directions
- **Deadly tiles**: Move each beat in configured direction, instant kill on contact

## Hard Constraints
- NEVER edit `.meta` files - these are Unity-managed asset references
- NEVER edit files in `Library/`, `Temp/`, `obj/`, or `Logs/` - auto-generated
- NEVER edit `ProjectSettings/` files directly - use Unity Editor
- Changing `[SerializeField]` fields can break prefab/scene references - flag these changes
- All scripts live in `Assets/Scripts/` (flat, no subfolders)
- Two control schemes: "Keyboard Left" (P1: WASD) and "Keyboard Right" (P2: arrows)

## WebGL Constraints (this is a WebGL-only project)
- NEVER use `AudioSettings.dspTime` - it is unreliable/unavailable in WebGL. Use `Time.timeAsDouble` instead
- NEVER use `System.Threading` or `Task`/`async`/`await` - WebGL is single-threaded with no threading support
- NEVER use `System.IO` file operations (File.Read/Write) - there is no filesystem access in WebGL
- NEVER use `System.Net.Sockets` or raw networking - use `UnityWebRequest` if HTTP is needed
- NEVER use `Application.Quit()` for actual exit - it has no effect in WebGL (OK in menu UI as a no-op)
- Avoid large blocking operations - WebGL runs on the browser main thread and will freeze the tab
- Compressed audio formats: use `.mp3` or `.ogg` (not `.wav`) to minimize download size
- Minimize asset sizes - everything is downloaded before play
- `PlayerPrefs` works in WebGL (uses browser IndexedDB) - safe for save data if needed
- All existing `#if !UNITY_WEBGL` / `#else` blocks should favor the WebGL path; clean up non-WebGL paths when possible

## Code Style
- PascalCase: classes, methods, properties, public fields
- camelCase: local variables, parameters, private fields
- `[SerializeField]` for inspector-exposed private fields
- Minimal comments - code is self-documenting
- No unit tests currently exist

## Build Target
- **WebGL only** - live at https://www.llauer.de/ggj-2024/
- All code must be WebGL-compatible (see WebGL Constraints above)
