# Asura: target

Status: draft target, 2026-09-25. Operator rulings are recorded at the end.
Where this document and the Body disagree, the Body wins and this document is
stale.

## Purpose

Rocky planets for Aetheria, in the style of a "tiny planet" photo: the scale is
deliberately distorted, so a desert planet's dunes are big enough to shape its
silhouette and its biome reads from orbit. Planets are seen from space. There is
also one close-up view when docked, showing an oversized station asset sitting on
the surface. There is no landing and no surface streaming.

Aetheria's volumetric gas bodies (`VolumeJupiter` and the rest) already work and
are not in scope.

## The machine

```text
planet definition (seed, radius, biome parameters)
  -> planet field  f(p) = sphere + large-scale biome displacement + noise      [HLSL]
  -> sampled grid over the planet's bounds                                      [GPU -> readback]
  -> surface nets: one vertex per crossed cell, one quad per crossed grid edge  [C#, CultLib]
  -> tile slot per quad in an atlas
  -> tile pass: per quad, an N x N grid of points refined onto f = 0,          [HLSL compute]
     with gradient normals and biome material
  -> patches rendered from the tiles (vertex positions read from the atlas)     [Built-in RP shader]
```

- **Large shapes live in the field, fine detail in the tiles.** Dunes that change
  the silhouette are resolved by the surface-net grid. The tile pass only refines
  detail smaller than one grid cell. Tiles hold real displaced positions, not
  normal-map illusions, because the silhouette is the point.
- **Hard-surface look:** normals come from the field gradient per texel. Brush
  seams get a small smooth-min bevel. Distant or unloaded quads fall back to
  face-weighted normals on the bare surface-net mesh.
- **Docked view:** the quads near the dock site get higher tile resolution. The
  station asset sits on a field query (height and normal along a direction). An
  optional flattened pad under it is blended in with a smooth-min.
- **Props** (oversized trees, rocks), chosen per biome and scattered along
  normals, come in a later cut on top of this one.

## Invariants

1. **The field is the only truth about shape.** The atlas is a cache, and
   everything in it can be regenerated from the planet definition. Nothing reads
   positions back out of the atlas as authority, and nothing writes the field
   from it.
2. **One implementation of each planet field, in HLSL.** The CPU never evaluates
   the planet field. Surface nets consumes a grid the GPU sampled, so there is no
   C#/HLSL parity to keep for the field itself. (Pending fork F1.)
3. **Noise comes from CultMath** (`cultmath_snoise(float3)`, which already has
   C#/HLSL parity tests). Asura does not grow its own noise. Missing noise
   capabilities go into CultMath.
4. **Surface nets is a CultLib algorithm.** It is engine-neutral C# in
   `GameCult.Geometry`, beside `CultGeometryIsoSurface.Extract`, and takes the
   same input shape (`float[,,]`, iso value, origin, cell size). Asura is only
   the Unity-side consumer.
5. **Quads are named by the grid edge they cross,** `(cell, axis)`. Two quads
   sharing an edge compute that edge's refined points identically: same corners,
   same parameterisation, same shader. So the surface is watertight wherever
   neighbouring quads use the same tile level.
6. **Reproducible from the seed.** The same definition gives the same field, the
   same quads and the same tiles, on any machine.
7. **Zone generation never builds geometry.** Zone gen writes planet definitions
   only. Asura builds a planet on demand, asynchronously, when it first needs to
   be seen, and the existing flat planet icon stands in until it's ready. Each
   planet's build is independent: no shared mutable state, so builds run in
   parallel. This is the lesson of the Celestial Body plugin, which was abandoned
   because it was too slow to run for many planets during every zone gen and
   could not be parallelised.

## Canonical implementations and consumers

| Piece | Owner | Runtime |
| --- | --- | --- |
| Noise | CultMath (`packages/cultmath`) | C# and HLSL, parity-tested |
| Surface nets | CultLib `GameCult.Geometry` | C#, netstandard |
| Planet field, grid sampling, tile pass, atlas, patch rendering | Asura (Unity package, `GameCult/Asura`) | HLSL and C#, Unity 6000.3, Built-in RP |
| Planet definitions, dock sites, which planets exist | Aetheria (content and zone data) | Unity |

**Not consumers:**
- `GameCult.Geometry.Csg`'s `distance(p)` (landed `d84acb6`) serves future
  brush-built structures. Planets do not use it.
- The third-party "Celestial Body" plugin in Aetheria is unused (no scene
  instantiates it) and is to be deleted (ruling F2).
- Aetheria's nebula field (`Volumetric.cginc`) is for later condensed bodies, not
  planets.

## Identity, lifecycle, authority

Every persistent or cached kind. Asura persists nothing in this campaign: every
row except the first is derived at load time and discarded on unload.

| Kind | What names it | What happens to it over time | Who decides |
| --- | --- | --- | --- |
| Planet definition (seed, radius, biome parameters) | Aetheria's planet or content identity | Authored or generated with the zone; immutable for a given seed | Aetheria content |
| Sampled field grid | (planet, grid resolution) | Sampled on planet load, read back, dropped once meshed | Asura field kernel |
| Surface-net quad | (planet, cell, axis) | Rebuilt from the grid on load, never persisted | CultLib surface nets |
| Tile slot | quad id to slot index, in an allocation table | Allocated on load; promoted to a higher level near the dock site; freed on unload | Asura atlas allocator |
| Tile contents (positions, normals, material) | the slot | Regenerable at any time from field and quad; overwritten on promotion | Asura tile pass. Forbidden writers: anything else |
| Dock site and station placement | Aetheria station identity | Authored | Aetheria. Asura only answers field queries |

## Scope boundary

In: the planet field, grid sampling, surface nets in CultLib, the tile atlas and
tile pass, patch rendering, orbital and docked levels, and the dock-site field
query.

Out:
- Landing and streaming levels of detail. The virtual-texture feedback and
  residency loop: the atlas is plain until measurements say otherwise.
- Runtime editing (cratering, mining).
- Props.
- Condensed bodies following the nebula field.
- Web builds.

Two separate campaigns are queued behind this one:
- The flow field as the single owner of motion in Aetheria.
- Microfauna species and resources in the slime-mold scene.

## Deferred paths

- **Dual contouring vertex placement** (discussed 2026-09-25). It gives the same
  topology and the same `(cell, axis)` names as surface nets; it differs only in
  where each vertex sits, a QEF fit to the tangent planes at edge crossings
  instead of the mean of the crossings.
  - Refinement onto `f = 0` recovers most of the difference. Dual contouring still
    wins in two places:
    - at N = 1, where the base vertex is the surface;
    - at creases, which normal-constrained projection cannot pull patch points
      onto.
  - It costs clustered or drifting vertices (clamped to the cell, biased toward
    the mass point), which means uneven texel density, plus gradient samples at
    every edge crossing.
  - Worth it for sharp-featured fields (brush structures, ridged noise), not for
    smooth planets. It is a later option inside CultLib's mesher behind the same
    output contract. Do not add a placement-strategy abstraction before a
    sharp-featured field exists to need it.

## Substrate facts this rests on (mapped 2026-09-25)

- **Aetheria:** mainline `F:\Projects\Aetheria`. Unity `6000.3.24f1`, Built-in
  RP. CultLib arrives as git UPM packages (`org.gamecult.cultlib` v1.0.60,
  `org.gamecult.cultmath` `cultmath-unity-v0.2.4`). There is no
  `GameCult.Geometry` package dependency yet. In-game planets are flat
  body/icon/gravity-well quads (`ZoneRenderer.cs`, `PlanetObject.cs`).
- **CultLib:** `CultGeometryIsoSurface.Extract` is marching tetrahedra with a
  clean-port provenance file. There is a `gamecult-geometry-unity-v0.1.0` tag,
  but no Geometry DLL in `org.gamecult.cultlib`. There is no Stryker config for
  `GameCult.Geometry` yet.
- **CultMath:** 3D simplex noise in C# and HLSL with parity tests. Value noise is
  2D only, and there are no analytic derivatives.

## Operator rulings

- 2026-09-25: Asura is for Aetheria (Unity), not Bevy.
- 2026-09-25: Planets first, rocky only. Seen from space, plus a docked
  close-up. Stylised tiny-planet scale; big features shape the silhouette.
- 2026-09-25: Surface nets, with subdivision and displacement restoring sharp
  features. Face-weighted normals for hard-surface edges.
- 2026-09-25, F2: the Celestial Body plugin is deleted in its own subtraction
  cut. It "was nice, but it was way too slow to generate a bunch of planets
  during every zone gen, and I wasn't able to parallelize it, so I just let it
  sit there." None of its code is harvested. Its shape ideas (craters, shattered
  bodies, moats) may be rebuilt fresh as field functions.
- 2026-09-25, budget: "Plenty of bodies in the solar system, but you're usually
  only close to a few of them. What with our exaggerated scale it could be a few
  dozen." And the Jevons clause: "if you make it fast enough that I can query it
  for a hundred new unique asteroids every frame, guess how many unique asteroids
  are gonna end up in frame." So the target is a throughput curve, not a latency
  number:
  - Resolution follows screen size per body. A near planet gets a large grid; a
    rock gets a small one.
  - Refined the same day: every body gets tiles, and the detail comes "from
    baked textures and displacement". A rock's base mesh is "a couple hundred
    polygons … maximum", which means grids of about 8³–12³ and roughly 100–300
    quads. Superseded the same day by the next ruling: "If you can see the
    texture, you can see the shape, we always need some displacement." There is
    one tile mode:
    - Every tile stores refined positions, gradient normal and material. The
      vertex shader always expands each quad into an N×N patch.
    - N is chosen per body from screen size. N = 1 is the tiny-rock case: the
      base mesh snapped onto the surface. Texel density scales with N.
    - N is uniform within a body, so the body is watertight by construction.
      The docked view raises N near the dock site, and that is where per-edge
      stitching enters scope.

    Face-weighted normals are only the stand-in while a body's tiles aren't
    ready.
  - The measure is bodies per millisecond at each grid size, benchmarked and
    committed. It is not the latency of one planet.
- 2026-09-25, F1 (Self's recommendation; the operator raised no objection): path
  A first.
  1. The planet field is HLSL only.
  2. The GPU samples each body's grid, with one batched readback per frame.
  3. CultLib's C# surface nets runs on worker threads, one body per task.

  The named-quad output is the seam between mesher and tile pass. Path B (surface
  nets as an HLSL mirror in CultLib, parity-tested, with no readback) slots in
  behind that seam if the throughput benchmark shows the readback round trip or
  CPU meshing is the cap.
- 2026-09-25, F3 (default recorded; no objection): bodies are generated once
  from their seed. There is no runtime editing in this campaign. Unique bodies
  are new seeds, not edits.
