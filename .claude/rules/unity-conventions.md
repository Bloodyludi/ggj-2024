# Unity Conventions for LAB RATS

## MonoBehaviour Lifecycle
- `Awake()` runs before `Start()` - use for self-initialization
- `Start()` runs after all Awake calls - use for cross-object setup (PlayerManager uses this)
- `OnEnable()`/`OnDisable()` - use for event subscription/unsubscription (standard pattern in this project)
- `OnDestroy()` - use for cleanup

## Reference Resolution
- **Manager cross-references**: Use `Services.Get<T>()` (e.g., `Services.Get<BeatManager>()`). Managers self-register in `Awake()` via `Services.Register(this)`.
- **Asset references** (prefabs, AudioClips, ScriptableObjects, UI, Transforms): Use `[SerializeField]` and wire in Inspector
- **Runtime-instantiated objects**: Use `Init()` method pattern (e.g., `PlayerController.Init()`, `DancefloorTile.Init()`)
- NEVER use `FindObjectOfType`, `GameObject.Find`, or `FindWithTag` for runtime lookups
- Use `[FormerlySerializedAs("oldName")]` when renaming serialized fields to preserve Inspector data

## Event Subscription
- Always subscribe in `OnEnable()` and unsubscribe in `OnDisable()`
- Never use the `-= +=` hack in `Awake()` (this was the old pattern, now removed)
- For runtime-instantiated objects, null-guard the event source in `OnEnable()` (it may not be set yet)

## Coroutines
- All timed animations use `CoroutineUtils.PacedForLoop()` (static shared utility)
- `PacedForLoop()` takes a duration and callback, yields `null` per frame, provides normalized progress [0..1]
- NEVER use `WaitForEndOfFrame` - it can cause issues in WebGL. Use `yield return null` instead.
- Stop coroutines before starting new ones on the same target to avoid conflicts
- Coroutines pause when `Time.timeScale = 0` (used by PauseScreen)

## Serialization
- `[SerializeField]` exposes private fields to the Unity Inspector
- Renaming a serialized field breaks all existing references in scenes/prefabs - use `[FormerlySerializedAs]`
- Adding new serialized fields is safe (defaults to type default)
- Removing serialized fields loses data but won't crash
- When changing serialized fields, always note this in your response so the user can verify in the Editor

## ScriptableObjects
- `SongLevelData` is the project's first ScriptableObject - created via Assets > Create > Lab Rats > Song Level Data
- Use ScriptableObjects for data that varies per-level/per-song but not per-frame
- Referenced via `[SerializeField]` on the component that needs it

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
- No `#if !UNITY_WEBGL` preprocessor blocks remain - use `Time.timeAsDouble` directly

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
- Manager-to-manager references use `Services.Get<T>()` (no Inspector wiring needed)
- Asset references (prefabs, clips, ScriptableObjects, UI) use `[SerializeField]` in Inspector
- Runtime objects (players, tiles) receive references via `Init()` method
- Players are instantiated at runtime via `PlayerInput.Instantiate()` with prefab
- Prefabs are in `Assets/Prefabs/`
- Never hardcode asset paths - use serialized references

## Performance Notes (WebGL-critical)
- Tile pulsing uses coroutines per-tile (40 concurrent coroutines during gameplay)
- Camera shake runs a coroutine on every beat
- No object pooling currently - deadly tiles modify existing tile state rather than spawning new objects
- WebGL runs on browser main thread - heavy operations cause visible frame drops
- Per-beat callbacks use reusable caches (Dictionary, HashSet, List) to avoid GC allocations
- No LINQ in per-beat or per-frame code paths
- Avoid `string.Format` / string concatenation in per-frame or per-beat code (use cached strings or StringBuilder)
