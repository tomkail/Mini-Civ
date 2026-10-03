# Improve fog of war

## Context

Fog is revealed by clicking: `GameController.Reveal` applies a rule per terrain type (mountains reveal radius 2, grass floods its connected grass area, others reveal one hex) and writes to `BoardModel.fogLayer` (`FogGridLayerModel` holding one `FogModel` per land hex). The look is a stencil mask: revealed cells → islands → outline polygon → extrude + smooth → stencil, then scrolling angled dashes everywhere outside it.

Fog stays click/rule driven. It shouldn't use `HexFieldOfView` (line of sight).

## Problems

1. **The fog on screen doesn't come from the model.** In `Board.unity`, `GameRenderer` (draws from `fogLayer`) is disabled. The enabled `FogRenderer` draws from its own inspector list `pointsToReveal`, which is empty, so clicking to reveal changes the model but nothing on screen.
2. **`FogRenderer` is a broken copy of `GameRenderer.DrawFog`.**
   - It calls `terrainTilemap.RefreshAllTiles()` every frame.
   - It seeds its `IslandDetector` with `HexCoord.OffsetToAxial(x, y)` but tests membership with `new HexCoord(x, y)` (offset vs. axial mismatch).
   - Its membership test is `fogTiles.Any(...)`, so O(n²).
   - It throws away the islands it finds and draws `pointsToReveal` instead.
   - Nothing writes `FogTile`s to `fogTilemap`, so that data is stale or hand-painted.
3. **Revealed areas with holes render wrong.** `OutlineDetector.GetOutlinePoly` traces one outer loop per island, so an unrevealed pocket inside a revealed area is filled in. There's a commented-out reproduction in `TomsLevelGenerator` ("This tests a bug where the fog has a hole in it").
4. **Everything is rebuilt every frame**, including in edit mode (`[ExecuteAlways]`): island detection, outline tracing, extrusion, smoothing and ear-clip triangulation run in `DrawShapes`, though fog only changes on a click or on level generation.
5. **Heavy model for one bit per cell.** A `GridEntity` per hex for a single `revealed` bool, plus a per-entity C# event that nothing subscribes to. `RevealFog(point, radius)` scans every fog entity per call, and `Reveal` calls it once per hex, so revealing an area costs O(area × map). Its `radius` parameter is never used.

## Proposed work

- [x] Draw fog from the model only. Delete `FogRenderer.cs`, `FogTile.cs` and the fog `Tilemap`, and re-enable `GameRenderer`'s fog (or move it to its own `FogRenderer` that reads `gameModel.board.fogLayer`).
- [x] Give `FogGridLayerModel` a fast path: a `HashSet<HexCoord>` (or dictionary) of revealed cells, `Reveal(IEnumerable<HexCoord>)` for batches, and one `OnChanged` event for the whole layer, not one per entity. Drop the per-cell `FogModel` entity if nothing else needs it. *(Done differently: `use-unityx` moved fog into `BoardModel.fog`, a `HexMap<bool>`; this added a batch `RevealFog(IEnumerable<HexCoord>)` and one `OnFogChanged` event to it.)*
- [x] Cache the fog shape. Rebuild it only when `OnChanged` fires or the renderer's settings change (`OnValidate`), and draw the cached polygons each frame.
- [x] Support holes. Port the approach in Resource-Claiming-Game's `TerritoryMeshBuilder`: trace every boundary loop (outer and hole) from the cell edges, offset and union with Clipper2, round the corners, then triangulate with Clipper2. That class assumes square cells (right-angle corners), so corner rounding needs generalising for hexes (120°/240°), or use Clipper's round joins. Copy and adapt it into Mini-Civ; it's not a shared package yet.
- [x] Re-enable the hole reproduction in `TomsLevelGenerator` as a test case (an edit-mode test would be best) and confirm the pocket stays fogged.
- [x] Optional: draw the cached shape as a `Mesh` with a stencil material instead of Shapes `PolygonPath`, so it isn't re-uploaded every frame.

## Done when

- Clicking to reveal updates the fog on screen, from the model.
- An unrevealed pocket inside a revealed area stays fogged.
- With no input, the fog does no per-frame rebuild work (check in the Profiler).
- `FogRenderer.cs`, `FogTile` and the fog tilemap are gone, or replaced by one renderer that reads the model.
