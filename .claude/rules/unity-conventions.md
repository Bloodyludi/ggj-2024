# Unity Conventions for LAB RATS

## MonoBehaviour Lifecycle
- `Awake()` runs before `Start()` - use for self-initialization and finding references
- `Start()` runs after all Awake calls - use for cross-object setup
- `OnEnable()`/`OnDisable()` - use for event subscription/unsubscription
- `OnDestroy()` - use for cleanup
- This project uses `GameController.OnCustomUpdate` event instead of `Update()` for game-tick logic

## Coroutines
- All timed animations use coroutines with `PacedForLoop()` (see Vector2Extensions.cs)
- `PacedForLoop()` takes a duration and AnimationCurve, yields frames, provides normalized progress
- Stop coroutines before starting new ones on the same target to avoid conflicts
- Coroutines pause when `Time.timeScale = 0` (used by PauseScreen)

## Serialization
- `[SerializeField]` exposes private fields to the Unity Inspector
- Renaming a serialized field breaks all existing references in scenes/prefabs
- Adding new serialized fields is safe (defaults to type default)
- Removing serialized fields loses data but won't crash
- When changing serialized fields, always note this in your response so the user can verify in the Editor

## Input System (New Input System Package 1.17.0)
- Input actions defined in `Assets/Controls.inputactions`
- Two control schemes bound at instantiation time in PlayerManager.cs:
  - "Keyboard Left" (Player 1): WASD
  - "Keyboard Right" (Player 2): Arrow keys
- Input handled via `PlayerInput.onActionTriggered` callback
- `InputSystem.DisableAllEnabledActions()` used for global input disable

## WebGL (sole build target - all code MUST be WebGL-compatible)

### Timing
- NEVER use `AudioSettings.dspTime` - unreliable in WebGL
- ALWAYS use `Time.timeAsDouble` for high-precision timing
- Existing `#if !UNITY_WEBGL` blocks are legacy - the WebGL path (`#else`) is the only path that matters
- When writing new timing code, just use `Time.timeAsDouble` directly (no preprocessor needed)
- Reference: DancefloorTile.cs already implements the correct pattern

### Threading & Async
- WebGL is **single-threaded** - no `System.Threading`, no `Task`, no `async`/`await`
- Use coroutines for all asynchronous-style operations
- Avoid large blocking loops - they freeze the browser tab

### Filesystem & Networking
- No `System.IO` file access - there is no local filesystem in WebGL
- No raw sockets (`System.Net.Sockets`) - use `UnityWebRequest` if HTTP needed
- `PlayerPrefs` works (backed by browser IndexedDB)

### Browser Constraints
- `Application.Quit()` is a no-op in WebGL (safe to keep for UI, but does nothing)
- All assets are downloaded before play - minimize sizes (use `.mp3`/`.ogg`, compress textures)
- `Screen.fullScreen` works but requires user gesture (click) to activate
- `Debug.Log` outputs to browser console - useful for debugging deployed builds

### Input in Browser
- Keyboard input works natively in WebGL
- Browser may capture some keys (F5, Ctrl+W, etc.) - game cannot override these
- Focus can be lost when clicking outside the game canvas

## Scene Management
- Scene transitions use `SceneManager.LoadScene(sceneName)`
- Scene names: "SampleScene" (gameplay), "MainMenu", "HowTo", "Credits"
- Game pause uses `Time.timeScale = 0/1` (not scene loading)

## Asset References
- Scripts find managers via `FindObjectOfType<T>()` or `FindWithTag()`
- Players are instantiated at runtime via `PlayerInput.Instantiate()` with prefab
- Prefabs are in `Assets/Prefabs/`
- Never hardcode asset paths - use serialized references or tags

## Performance Notes (WebGL-critical)
- Tile pulsing uses coroutines per-tile (40 concurrent coroutines during gameplay)
- Camera shake runs a coroutine on every beat
- No object pooling currently - deadly tiles modify existing tile state rather than spawning new objects
- WebGL runs on browser main thread - heavy operations cause visible frame drops
- Minimize GC allocations in hot paths (beat events fire every ~0.5s at 120 BPM)
- Avoid `string.Format` / string concatenation in per-frame or per-beat code (use cached strings or StringBuilder)
