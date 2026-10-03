# CLAUDE.md

Guidance for working in the **Mini-Civ** Unity project.

## Unity MCP server — which one to use

Two Unity MCP servers may be connected in this environment. **When I say "the Unity MCP server", I always mean the community "MCP for Unity" server, NOT the official Unity one.**

- ✅ **Use this one** — tools prefixed `mcp__UnityMCP__*` (e.g. `mcp__UnityMCP__read_console`, `manage_editor`, `manage_gameobject`, `manage_scene`, `execute_code`). This is the CoplayDev "MCP for Unity" server.
- ❌ **Not this one** — tools prefixed `mcp__unity-mcp__*` (e.g. `Unity_RunCommand`, `Unity_Camera_Capture`). This is the official Unity server; don't use it unless I explicitly ask for "the official one".

If the Unity Editor isn't attached, `mcp__UnityMCP__*` tools return `no_unity_session` even though the server itself is up — ask me to open the Editor rather than assuming the server is broken.

## Project overview

A small civ-style exploration prototype on a hex grid: a procedurally generated island map (grass, forest, mountain, river, road) under fog of war; the player clicks hexes to move along an A* path and reveal fog. Unity 6000.5, Built-in render pipeline. All game code is in the default `Assembly-CSharp` (no project asmdefs).

- `Assets/Scenes/Board.unity` — the playable scene: `Game` (GameController + GameInputController), `Grid` (WorldSpaceHexGrid, MasterGrid, terrain/fog/item Tilemaps), the level generator, GameRenderer/FogRenderer and a RadialGradientRenderer background. `Main.unity` holds UI/bootstrap objects. `Game.asset`/`Main.asset` are UnityX `RuntimeSceneSet`s.
- Hex maths come from UnityX's `hex` package (namespace `UnityX.HexGrid`: `HexCoord`, `HexUtils`, `HexEdgeDirections`, `WorldSpaceHexGrid`, plus `HexSearch`/`HexMap`/field of view that the game doesn't use yet). `Assets/Scripts/Grid/` keeps `MasterGrid` and `WorldSpaceHexGridExtensions.XYPlaneRotation()` — use that, not `WorldSpaceHexGrid.axis`, to lay XY/Shapes drawing onto the board (the package's `axis` is a different frame).
- `Assets/Scripts/Model/` — game state (`GameModel`, `BoardModel`, grid layers for land and fog, entities such as `TerrainModel`/`FogModel`).
- `Assets/Scripts/LevelGenerator/` — `GameController` (`[ExecuteAlways]`; regenerates the level whenever it's enabled, including in edit mode), `TomsLevelGenerator` (+ settings asset with an optional fixed seed), `PathFinder` (UnityX `AStar`), input.
- `Assets/GameRenderer.cs`, `Assets/FogRenderer.cs` — Shapes immediate-mode drawing of the cursor, path and fog outlines (uses UnityX islands/outline detection). `Assets/Tiles/` — `Tile` subclasses for terrain, fog and items.
- Third-party: `Assets/Plugins` (Shapes, UI Shapes Kit, TriangleNet, unity-spring); `Assets/Tilemap` is the 2D Tilemap Extras samples. EasyButtons and UniTask come from git packages.
- The project was started from a copy of Sea-Rising: many prefabs (`Assets/Prefabs`), `Assets/Settings/*.asset` and some `Main.unity` objects still reference Sea-Rising scripts that don't exist here (missing scripts), so Unity refuses to resave those prefabs.

## UnityX (submodule)

`UnityX/` is a git submodule of github.com/tomkail/UnityX. Its `Packages/com.tomkail.unityx.*` folders are referenced as local `file:` packages from `Packages/manifest.json` — manage them with `UnityX/Tools/unityx` (`list`, `add`, `remove`, `sync`, `status`, `update`, `scan`), which also writes each package's dependencies into the manifest. The code is editable in place: to change UnityX, `cd UnityX && git checkout master`, commit and push there, then commit the submodule bump here (`git add UnityX`). See `UnityX/README.md`.

## Conventions

- After creating/editing scripts, check `mcp__UnityMCP__read_console` for compilation errors before using new types, and poll the editor state's `isCompiling` flag to know when domain reload finishes.
- Heads-up: other agents sometimes run broad `git add -A` / commits on shared branches — be careful staging changes.

## Checking the Unity editor

The editor is often open while we work. To check whether it's running, list process **names only**:

```bash
pgrep -l Unity
```

Don't use `ps aux`, `ps -ef`, `pgrep -f`/`-lf` or anything else that prints full command lines. Unity Hub launches the editor with the account's sign-in token on its command line, so printing full arguments puts the token into the transcript.

## Compiling without the editor

Use the C# compiler bundled with the editor version in `ProjectSettings/ProjectVersion.txt`, if it's installed. The `.csproj` files at the repo root (generated when the editor opens the project) list every source file, reference and define, so build a response file from one and pass it to `csc`:

- Compiler: `/Applications/Unity/Hub/Editor/<version>/Unity.app/Contents/Resources/Scripting/DotNetSdk/dotnet <same folder>/DotNetSdk/sdk/<sdk version>/Roslyn/bincore/csc.dll @file.rsp`
- From the csproj, take `<Compile Include>` (sources), `<HintPath>` (references), `<ProjectReference>` (resolve to `Library/ScriptAssemblies/<name>.dll`) and `<DefineConstants>`. Add `-target:library -nostdlib+ -noconfig -unsafe+`.
- Write the output to the scratchpad, never into `Library/`.
