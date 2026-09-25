# Asura: cut map

Status: cut map, Imagination pass 1, 2026-09-25. Nothing here has landed.
`docs/target.md` owns the ends. This document owns the means. Where this map and
the Body disagree, the Body wins and this map is stale. The map lives on `main`
of `GameCult/Asura`.

Pinned HEADs (every `file:line` below is against these):

| Repo | Ref | SHA | Note |
| --- | --- | --- | --- |
| Asura | `main` | `54a748a` | target with the one-tile-mode and crease-snapping rulings |
| CultLib | `origin/main` | `29e50ad` | Local `main` in `F:\Projects\CultLib` is 15 commits behind (`d110d9d`). The main checkout sits on `hands/cultmath-erf`. Work from `origin/main` in a worktree. |
| CultLib tags | `cultmath-unity-v0.2.4` / `cultlib-unity-v1.0.60` | `6d5e209` / `45c2f40` | both are ancestors of `29e50ad` |
| Aetheria | `origin/master` | `9b85211f` | mainline, 2026-09-17; pins `cultlib-unity-v1.0.59` and `cultmath-unity-v0.2.3` |
| Aetheria | `origin/codex/fire-control-12` | `ab14552a` | In flight: 218 commits ahead of master and 2 ahead of the local checkout (`14ee2a52`). It pins cultlib `1.0.60`, cultmath `0.2.4` and caching `1.4.0`. **Do not touch `F:\Projects\Aetheria`'s tree.** |

Rulings: the target's dated rulings, through the 2026-09-25 crease-snapping
ruling. Nothing further yet.

Open: operator questions Q1–Q9 at the end. Q1 blocks Cuts 1 and 7. Q3, Q5 and Q6
block Cut 7. Q4 blocks Cut 2a's scope.

## Cut order

```text
2a CultMath gaps ─┐
2  surface nets ──┴─> 3 CultLib Unity release ─> 4a Asura skeleton ─> 4b field + sampling
                                                   ─> 5a mesh + bare render ─> 5b benchmark
                                                   ─> 6a tile pass ─> 6b patch render + watertight ─> 6c crease snap
                                                   ─> R Asura release tag
1 delete plugin (Aetheria; any time after Q1) ─────────────────────────> 7a defs ─> 7b integration ─> 7c docked view
```

Re-splits from the brief, with reasons:
- **2a is new.** CultMath has no fBm, no smooth-min and no cellular noise. The
  HLSL function list at `29e50ad` is catmullrom, clamp, csum, the béziers, damp,
  decay, degrees, distance, frac, hash, lengthsq, lerp, pcg/pcg3d/pcg4d, reflect,
  rotate, saturate, the smoothsteps, snoise, step and value_noise*. The target
  needs fBm for its noise and smooth-min for its bevels and pads. Gaps are filled
  in the owner, and 2a releases before Cut 3, because Cut 3's Geometry DLL is
  compiled against the CultMath version it ships beside (see Cut 3).
- **Cut 4 is split** into the skeleton (package, host project, headless Core
  tests) and the GPU work. Each part alone is roughly 150–200k tokens.
- **Cut 5 is split** into the build path and the benchmark. The benchmark has
  its own harness and its own committed artefact.
- **Cut 6 is split three ways**, as the operator asked: the compute pass,
  then rendering with a watertightness proof at uniform N, then crease snapping.
- **R (Asura release tag)** is its own cut, because a tag is a cut and gets its
  Soul pass before it is pushed (SKILL.md step 5).
- **Cut 7 is split** into definitions (headless, mutation-reachable), then
  render integration, then the docked view.
- **Cut 1** runs whenever Q1 is settled. Nothing in Cuts 2–6 depends on it.

## Standing design decisions (means, not operator forks)

These apply across cuts. Self may overrule any of them. None is a product fork.

- **D1. The quad mesh stays a plain CultLib value, not a document.** The
  surface-nets output is `CultGeometryQuadMesh`, a sealed class with no
  `[MessagePackObject]` and no `[CultDocument]`. It is not a struct, because a
  struct's `default` would carry null arrays. Asura persists nothing (target
  identity table), so the mesh has no wire contract to keep.
- **D2. Face-weighted normals belong in CultLib**, beside surface nets, as a pure
  function of the quad mesh. Four reasons:
  - The function is engine-neutral: quads in, per-vertex normals out.
  - The mesher owns the topology the normals are computed over.
  - It runs on the same worker thread as the mesher, so there is no extra
    handoff.
  - In CultLib it is reachable by Stryker; in Asura it would be Unity code that
    no mutation tool reaches.

  Unity's `Mesh.RecalculateNormals` is not a substitute, because its weighting is
  not the rule the operator ruled.
- **D3. The field is unit-radius in body-local space.** Asura's mesh is a child
  of Aetheria's `Body` transform, which already carries `localScale = BodyRadius`
  (`ZoneRenderer.cs:406` at master). This depends on the Q5 answer.
- **D4. Patch points are stored once per shared element.** A body's tile data
  is three regions, each keyed by the thing it belongs to:
  - one refined point per base vertex;
  - `E−1` refined points per unique mesh edge;
  - `(N−1)²` interior points per quad.

  A mesh edge joins two surface-net vertices, so it is dual to one grid face.
  Its key is the vertex-index pair, lower index first. The patch expansion reads
  corners, edges and interior from these regions, so neighbouring quads read the
  *same memory* for their shared edge. Watertightness then holds by
  construction, not by floating-point determinism between two invocations. The
  same edge-keyed region carries the docked view's per-edge factor (Cut 7c).
  This is how invariant 5 is made impossible to violate rather than merely
  likely to hold.
- **D5. The atlas is per body.** A body's tile storage is one `GraphicsBuffer`
  sized from its quad count and N. Its "allocation table" is the identity: slot
  = the quad's index in the mesher's sorted output. It is freed by disposing the
  buffer on unload. This satisfies the target's identity row ("quad id → slot
  index; freed on unload"). A shared atlas with a block allocator waits until the
  benchmark shows draw or buffer count is the cap. Record the decision; do not
  build the allocator.
- **D6. Asura keeps a headless Core.** Rules with no GPU in them live in a
  `noEngineReferences` assembly: level and N selection, the remesh/retile
  decision, mesh-edge derivation, the patch layout offsets and the build job
  body. A small `net10.0` test project compiles those sources, the way
  `Aetheria.Shared` compiles ServerShared. This is the only way Stryker reaches
  Asura. GPU rules are defended by readback checks at the layer where they fail.
- **D7. Unity tests run in a host project inside the Asura repo**
  (`unity/AsuraHost/`), following CultLib's own `src/GameCult.Unity` host
  project. Aetheria's tree stays out of the loop until Cut 7.

## Verification hosts (applies to every cut)

- **Yggdrasil** (`C:\Users\Meta\.claude\skills\eureka\tools\stopgap\ygg-verify.sh`,
  image `dotnet`) runs every .NET build, test and Stryker run: CultLib Geometry,
  CultMath, `Asura.Core.Tests` and `Aetheria.Shared.Tests`. Example:
  `ygg-verify.sh /f/Projects/CultLib <rev> dotnet 'dotnet tool restore && cd tests/GameCult.Geometry.Tests && dotnet test && dotnet stryker --since:<base>'`.
  Keep the job's own exit status last; see the script's header.
- **The Windows workstation (Starfire)** runs anything Unity, anything that needs
  the GPU, and the CultLib Unity package build. The package build runs
  `build-quic-native.ps1`, which is Windows-native. Rules for Starfire:
  - One job at a time.
  - Run detached, with a log in the scratchpad and the PID recorded, and poll the
    log.
  - Batchmode refuses a project that is open in the editor.
  - **Never pass `-nographics` to a GPU test.** Unity's docs for 6000.3 say that
    in batch mode with `-nographics` "Unity doesn't initialize the graphics
    device".
  - Revert incidental asset churn after every run.
- **Shader compile probe (done in this pass):** `CultMath.hlsl` at `29e50ad`,
  included into a `cs_5_0` kernel that sums six octaves of
  `cultmath_snoise(float3)`, compiles cleanly with FXC
  (`Windows Kits\10\bin\10.0.26100.0\x64\fxc.exe /T cs_5_0`). FXC is the compiler
  Unity uses for D3D11 compute. Aetheria's Windows APIs are D3D11 first, then
  D3D12 (`ProjectSettings.asset:350`, `m_APIs: 0200000012000000`, not automatic),
  and the render pipeline is Built-in (`GraphicsSettings.asset:41`,
  `m_CustomRenderPipeline: {fileID: 0}`). No Aetheria shader includes
  `CultMath.hlsl` yet, so Asura is its first Unity compute consumer.
- **AsyncGPUReadback:** the 6000.3 script reference states no render-pipeline
  requirement. `Request(GraphicsBuffer, size, offset, callback)` exists, and the
  docs say readback "adds a few frames of latency". Platform support is exposed
  only through `SystemInfo.supportsAsyncGPUReadback`. That is a runtime fact, so
  Cut 4b asserts it on Starfire's device rather than assuming it.

---

### Cut 1. Aetheria: delete the Celestial Body plugin (subtraction only)

- **Repo/branch:** Aetheria, `hands/asura-cut1`, based per Q1: master
  `9b85211f`, or the merged fire-control line. Anchors are given for both; the
  plugin's own files are identical between master and `ab14552a`.
- **First:**
  - A worktree. **Never** `F:\Projects\Aetheria` itself.
  - The worktree needs LFS objects for the files it edits, because `*.asset` is
    LFS (`.gitattributes`); `Settings.asset` is a 130-byte pointer in git. Run
    `git lfs pull --include` for those paths.
  - The Unity compile check needs a full LFS checkout and a Library import. That
    is long. Self decides whether Hands reuses an existing Aetheria worktree.
  - Record the negative-grep baselines below before any edit.
- **Deletes first:**
  - `Assets/Plugins/Celestial Body/`: 310 files, 11,344 text lines (C# 3,221;
    `.meta` 3,051; `.mat` 1,639; `.shader` 1,342; `.cginc` 1,219; `.compute` 755;
    `.asset` 111, all LFS pointers; `.txt` 6), plus binaries (6 tif, 4 psd, 3 png,
    3 jpg, all LFS). Also `Assets/Plugins/Celestial Body.meta` (8 lines).
  - `Assets/Scripts/Zone Display/PlanetObject.cs:14`: the
    `CelestialBodyGenerator Generator` field.
  - `ZoneRenderer.cs:40`: the `LODHandler LODHandler` field.
  - `ZoneRenderer.cs:397-401`: the body-settings pick. Keep `:396`
    (`planet = Instantiate(Planet, ZoneRoot);`) and `:402`, which is a comment.
    This pick is also a live nondeterminism: planet appearance is chosen with
    `UnityEngine.Random`, not the zone seed. It dies here and is not replaced.
  - `ZoneRenderer.cs:423`: `LODHandler.FindPlanets();`.
  - `Assets/Scripts/Gameplay/GameSettings.cs:38`, the `BodySettingsCollections`
    field, and `:84-89`, the `[Serializable] class BodySettingsCollection`. Lines
    are for master. On `ab14552a` they are `:34` and `:79-84`, because settings
    Cut 0 landed there.
  - `Assets/Resources/Settings.asset:77-98` (smudged): the
    `BodySettingsCollections:` block of 6 collections and 7 references, all into
    the plugin. Edit the text with Unity closed and commit through LFS.
  - `Assets/Prefabs/RPG/Planets/Planet.prefab`:
    - the disabled generator component `--- !u!114 &-7153121627769399805` (`:221-240`,
      `m_Enabled: 0` at `:228`);
    - its `- component:` entry on GameObject `&8859079004937162791`;
    - the `Generator:` line at `:220`.

    The `Terrain Mesh` body (`high-res-sphere.fbx` with `Assets/Materials/Planet.mat`),
    `Minimap Icon` and `Gravity Well` stay.
  - `Assets/Scenes/ARPG.unity`:
    - the enabled `LODHandler` component `--- !u!114 &1009503736` (`:16782-16795`);
    - its `- component:` entry on GameObject `&1009503727`;
    - ZoneRenderer's `LODHandler: {fileID: 1009503736}` at `:16721`.
  - **Per Q2:** `Assets/Resources/Prefabs/Stations/PlanetOutpost.prefab` (+meta)
    and `Assets/Resources/Locations/ReconStationAlpha.{prefab,asset}` (+metas).
    What they reference:
    - `PlanetOutpost.prefab:322-332` has an **enabled** `CelestialBodyGenerator`
      whose `body` is `ReconStationAlpha.asset`, and `:225` uses the plugin's
      `Watchful Eye.mat`.
    - `ReconStationAlpha.asset` is itself a `CelestialBodySettings` instance
      (`:12`, script guid; `:15-16` reference plugin Shape and Shading assets).
    - `ReconStationAlpha.prefab:90` uses `Watchful Eye.mat`.

    No code, prefab, scene or asset references either prefab by guid. The content
    catalog (`GameData/Aetheria.cc`) is not plain text (no `Assets/Resources`
    strings survive a byte grep), so a string search cannot clear it. Hands must
    decode it (see verification) before deleting. If a live consumer appears,
    stop: that is a fork.
- **Keeps:**
  - Everything else under `Assets/Plugins` (GPU Noise, LowPoly_AsteroidsPack and
    the rest).
  - `PlanetObject.Body/GravityWell/Icon`.
  - The gas-giant and sun branches of `LoadPlanet` (`:367-393`).
  - Nothing from the plugin is harvested (ruling F2).
- **Authority map:** subtraction only. No owner changes. Planet appearance
  becomes the stand-in sphere until Cut 7b.
- **Verification (Starfire, one job):**
  - builds: a Unity batchmode compile of the worktree (no GPU needed, so
    `-nographics` is allowed here) with zero compile errors. Also an
    `Aetheria.Shared` headless build on Yggdrasil. It is unaffected, but it
    proves ServerShared never touched the plugin.
  - catalog: on the fire-control line, `EngineAssetCheck.Run` in batchmode
    (`docs/addressables-cut.md:220`) exits 0 after the delete. On master, a
    scratch decode of `GameData/Aetheria.cc` through AetherDb's cache-open path
    must find 0 string fields containing `PlanetOutpost`, `ReconStationAlpha` or
    `Celestial Body`, and print the count of strings scanned. The probe stays
    scratch.
  - negative, verified against master today:
    - `rg -n -i "celestial" Assets ProjectSettings Packages -g "!*.md"` → 0. It
      hits only `GameSettings.cs:88` and `PlanetObject.cs:14` outside the plugin
      now.
    - `rg -n -w "LODHandler|BodySettingsCollections?|CelestialBody\w*" Assets` → 0.
      `TextureSize` in `Slime.cs:27` is an unrelated field; do not grep it.
    - The GUID sweep: build the list of `guid:` values from the plugin's 174
      `.meta` files **at the base commit**, then
      `rg -F -f <list> Assets ProjectSettings` over the smudged tree → 0. Today it
      hits 6 files: `Planet.prefab:230`, `PlanetOutpost.prefab:225,322`,
      `ReconStationAlpha.prefab:90`, `ARPG.unity:16791`,
      `ReconStationAlpha.asset:12,15,16` and `Settings.asset:79-95`. Run it on the
      smudged tree, because `git grep` cannot see into LFS `*.asset` files.
  - operator: open ARPG, enter a zone, and see rocky planets render as the
    stand-in sphere with no missing-script warnings in the console.
- **Rules that lose a test:** none. The plugin had no tests.
- **Ledger estimate:** −310 files, −11,344 text lines, −16 LFS binaries,
  −~15 C# lines, −~45 YAML lines. Add −2 prefabs and −1 asset if Q2 is A.
- **Hands budget:** ~120k tokens.
- **Doc sweep for Self:** `docs/settings-globals-cut.md` fork B (`:527-535`),
  which moves the 7 body-settings assets into `Resources`, is superseded by F2.
  Mark it history in that map the day this cut lands.

### Cut 2a. CultMath: fBm, smooth-min (and cellular noise per Q4)

- **Repo/branch:** CultLib, `hands/cultmath-asura-noise`, from `origin/main`
  `29e50ad`, in its own worktree. Release `cultmath-unity-v0.3.0`: additive, so a
  minor bump per `docs/semver-policy.md`. The release belongs to this cut and
  gets a Soul pass before the tag.
- **Adds,** each in C# `math` (`packages/cultmath/src/CultMath/math.cs`, near
  `snoise` at `:594`) and in `shaders/CultMath.hlsl` with the same name prefixed
  `cultmath_`:
  - `fbm(float3 p, int octaves, float lacunarity, float gain)`: the octave sum of
    `snoise(float3)`.
  - `smin(float a, float b, float k)`: polynomial smooth minimum. It feeds the
    target's brush-seam bevels and the dock pad blend.
  - Per Q4 = A: `cellular(float3 p)`, returning the nearest-feature distance, and
    `cellular_id` or an out parameter for the feature cell id. It is built on
    `pcg3d`. It is needed for craters.

  Mirror bodies must stay inside the transformations listed in
  `packages/cultmath/docs/design.md:92`.
- **Keeps:** everything. Both tracked copies of `CultMath.hlsl`
  (`packages/cultmath/shaders/` and
  `packages/cultmath/unity/org.gamecult.cultmath/Shaders/`) are the same blob
  (`6179d45`) and must stay identical. The existing package script already
  enforces this.
- **Verification (Yggdrasil):**
  - `HlslSourceCompatibilityTests.EveryMirrorFunctionMatchesCSharpMath`
    automatically compares every `cultmath_*` mirror with a C# counterpart bit
    for bit. A new mirror with no C# twin fails `Assert.Contains(... MirrorOnly)`.
  - Behavioural tests:
    - `fbm` with 1 octave equals `snoise`;
    - gain and lacunarity act per octave;
    - `smin(a,b,k) ≤ min(a,b)`, and it equals `min` when `|a−b| ≥ k`;
    - `cellular` distance is ≥ 0 and zero at a feature point, and the id is
      constant within a cell.
  - Stryker, `-t mtp --since:29e50ad`, from `packages/cultmath/tests/CultMath.Tests`
    (xunit.v3 needs `mtp`, per `docs/mutation-testing.md`). Triage every survivor.
  - Starfire: FXC `cs_5_0` compile of a kernel calling each new mirror function.
    This is the probe above, extended; FXC is Unity's D3D11 compiler.
- **Rules that must die:** octave count honoured; gain applied multiplicatively
  per octave; `smin`'s blend region bounded by `k`; `cellular` is the nearest
  feature, not the first one found.
- **Ledger estimate:** +~90 C#, +~70 HLSL, +~150 test lines.
- **Hands budget:** ~150k tokens, including the release.

### Cut 2. CultLib: surface nets and face-weighted normals in GameCult.Geometry

- **Repo/branch:** CultLib, `hands/asura-surface-nets`, from `origin/main`
  `29e50ad`, in its own worktree. No release in this cut; Cut 3 releases.
- **Do not touch:** `CultGeometryIsoSurface.cs` (`Extract`, marching tetrahedra,
  unwelded, `:39-101`), `CultGeometryDocuments.cs` (including
  `CultGeometryTriangleMesh`, `:373-400`) and `ISOSURFACE-PROVENANCE.md`.
- **Adds, in `src/GameCult.Geometry/`:**
  - `CultGeometrySurfaceNets.Extract(float[,,] samples, float isoValue = 0f,
    CultVec3 origin = default, float cellSize = 1f) → CultGeometryQuadMesh`. This
    is the same parameter list and validation as `CultGeometryIsoSurface.Extract`
    (`:39-54`): null, fewer than 2 samples on any axis, and a non-finite or
    non-positive cell size all throw the same exception types.
  - `CultGeometryGridEdge`: a readonly struct `{ int X, Y, Z; byte Axis }`. It is
    the grid edge from sample `(X,Y,Z)` to `(X,Y,Z)+e_Axis`, which is the
    target's `(cell, axis)`. It is `IEquatable` and `IComparable`, ordered by
    `(X, Y, Z, Axis)`.
  - `CultGeometryQuadMesh`, a sealed class and not a document (D1), with:
    - `Positions` (`float[]`, xyz per welded vertex);
    - `Quads` (`uint[]`, 4 vertex indices per quad);
    - `QuadEdges` (`CultGeometryGridEdge[]`, one per quad, strictly ascending);
    - `QuadCount` and `VertexCount`.
  - `CultGeometryQuadNormals.FaceWeighted(CultGeometryQuadMesh) → float[]`
    (xyz per vertex, D2). Each quad contributes its diagonal cross product
    `(d1 × d2)`, whose length is twice the area of a non-planar quad, so the
    direction and the area weight come together. Zero-area quads contribute
    nothing. A vertex whose sum is zero gets a zero normal, matching `Extract`'s
    degenerate rule at `:175-177`.
  - `SURFACE-NETS-PROVENANCE.md`, packed like `ISOSURFACE-PROVENANCE.md`
    (`GameCult.Geometry.csproj` `None Include ... Pack`). It cites:
    - S. Gibson, "Constrained Elastic Surface Nets", MICCAI 1998 (MERL TR99-24);
    - M. Lysenko, "Smooth Voxel Terrain (Part 2)", 0fps.net, 2012;
    - N. Max, "Weights for Computing Vertex Normals from Facet Normals",
      J. Graphics Tools 4(2), 1999.

    It states that the implementation is fresh and copies no code or tables. The
    deliberate behaviour it records:
    - inside is `<= iso`;
    - the vertex is the mean of the linearly interpolated edge crossings of its
      cell;
    - Gibson's elastic relaxation is omitted;
    - one quad is emitted per crossing interior edge;
    - vertices are welded.
  - `tests/GameCult.Geometry.Tests/SurfaceNetsTests.cs` and
    `QuadNormalsTests.cs` (NUnit, like `IsoSurfaceTests.cs`).
  - `tests/GameCult.Geometry.Tests/stryker-config.json`:
    - `"project": "GameCult.Geometry.csproj"`;
    - `"mutate": ["**/src/GameCult.Geometry/**/*.cs"]`;
    - `"mutation-level": "Standard"` and the same reporters and thresholds as
      CultMath's (`break: 0`);
    - **no** `"test-runner": "mtp"`. This project is NUnit 4 with
      `NUnit3TestAdapter` and `Microsoft.NET.Test.Sdk`, so Stryker's default
      `vstest` runner applies.
  - `docs/mutation-testing.md`, Follow-ups: move `GameCult.Geometry` from the
    unscoped list to scoped (diff-scoped only). A full-project Geometry baseline
    is a recorded follow-up, not this cut.
- **Contract rules. Each gets a behavioural test, and each must die under
  mutation:**
  1. **Sign convention matches Extract.** A sample `== iso` is inside. Fixture:
     the same field with one sample set to exactly `iso` and to `iso − ε`
     produces identical quads. `iso + ε` must differ, which kills `<` for `<=`.
  2. **Naming.** A quad's `QuadEdges` entry is the grid edge whose endpoints
     straddle iso. Fixture: a 3×3×3 grid with one inside sample at `(1,1,1)`
     gives exactly 6 quads. Their edges are `(1,1,1,a)` and `(1,1,1)−e_a` for
     `a = 0..2`. Also use a non-cubic grid (e.g. 4×3×5) with an off-centre
     inside blob, so axis and dimension swaps change the answer.
  3. **Orientation.** Every quad's `(d1 × d2)` has a positive dot product with
     `(outside endpoint − inside endpoint)` of its edge. The single-sample
     fixture exercises both cases: inside at the lower end (`+a` edges) and
     inside at the upper end (`−a` edges).
  4. **Vertex placement.** A vertex is the mean of its cell's interpolated
     crossings:
     - a tilted plane pins interpolation against midpoints;
     - a curved-field cell whose crossings sit at unequal positions pins the mean
       against the first crossing;
     - `origin` and `cellSize` are applied, reusing `Extract`'s determinism test
       shape (`IsoSurfaceTests.cs:67-87`).
  5. **Welding and closure.** On a closed field (a sphere with a one-sample
     margin):
     - every undirected quad edge is used by exactly 2 quads;
     - `VertexCount − QuadCount == 2` (Euler characteristic for genus 0).

     Probed in this pass with a scratch counter over sphere grids of 8, 10, 12,
     16, 32 and 64 per axis: `V − Q = 2` and 0 non-manifold edges at every size.
     Quads come out at 72, 192, 312, 672, 3,696 and 16,968. So the operator's
     "couple hundred polygons" rock is a 10³–12³ grid. Assert the invariants,
     not these counts. On an ambiguous cell (checkerboard) the edge count is
     **even**, not 2; a test states that too, so nobody "fixes" surface nets
     into claiming manifoldness.
  6. **Open boundary.** An edge whose 4 surrounding cells do not all exist emits
     no quad. Fixture: a plane crossing the whole grid yields quads only on
     interior edges, and the mesh is open. This is documented, not hidden.
  7. **Determinism and order.** Two calls give equal arrays, and `QuadEdges` is
     strictly ascending. Asura's slot index is the quad index (D5), so order is
     contract.
  8. **Face-weighted normals:**
     - a large quad and a sliver sharing a vertex give a normal within ε of the
       large quad's;
     - a closed cube-like mesh gives unit normals;
     - a zero-area quad contributes nothing, and a vertex with all-zero
       contributions gives a finite zero vector, not NaN.

     A mutant that drops the area weight (normalising each face first) must
     fail.
- **Authority map:**
  - Owner: `GameCult.Geometry` owns surface-nets topology, naming, orientation
    and face-weighted normals.
  - Inputs: a sampled grid.
  - Outputs: `CultGeometryQuadMesh`, plus normals on request.
  - Derived: nothing persisted.
  - Forbidden writers: Asura must not re-derive quad naming or orientation. It
    consumes them.
- **Verification (Yggdrasil):**
  - `dotnet test tests/GameCult.Geometry.Tests`; paste the per-file test counts.
  - `dotnet tool restore`, then from `tests/GameCult.Geometry.Tests`
    `dotnet stryker --since:29e50ad`. Triage every survivor by name. The
    `static readonly` caveat in `docs/mutation-testing.md` applies to the `mtp`
    runner, not `vstest`.
  - negative: `rg -n "MessagePackObject|CultDocument" src/GameCult.Geometry/CultGeometrySurfaceNets.cs src/GameCult.Geometry/CultGeometryQuadNormals.cs` → 0.
- **Ledger estimate:** +~220 C#, +~300 test lines, +1 provenance file, +1
  Stryker config.
- **Hands budget:** ~200k tokens.

### Cut 3. CultLib → Unity: GameCult.Geometry ships in `org.gamecult.cultlib`

- **Repo/branch:** CultLib, `hands/cultlib-unity-geometry`, from `main` after
  Cuts 2a and 2. Release `cultlib-unity-v1.1.0`: adding an assembly is additive,
  so a minor bump. Soul runs before the tag is pushed.
- **Decision (default A; Self may put it to the operator):** ship
  `GameCult.Geometry.dll` inside the existing `org.gamecult.cultlib` package, not
  in a new `org.gamecult.geometry`. The reasons:
  - Geometry depends on `GameCult.Caching` and MessagePack, which that package
    already ships.
  - The release machinery exists: the semver check, the deterministic build, the
    version check and the template update.
  - Aetheria then gains no new UPM dependency for it.
- **Not a source:** tag `gamecult-geometry-unity-v0.1.0` (`b65619f`) and branches
  `origin/codex/geometry-cultmesh-unity-release` and
  `origin/codex/geometry-ownership-migration`. They carry an unmerged July
  planetary line: `Planetary*` cube-sphere heightfield plus erosion,
  `unity/org.gamecult.geometry` and HLSL pages. No mainline consumes it.
  Fensalir `HEAD 58107ff` references only CultMath, and Aetheria references
  neither. Do not build on it, and do not delete it; see findings.
- **Per-file changes against `29e50ad`:**
  - `scripts/build-unity-package.ps1`:
    - add a publish of `src\GameCult.Geometry\GameCult.Geometry.csproj`
      (netstandard2.1 via `src/Directory.Build.props`), shaped like the WebSocket
      publish at `:77-85`, with the same deterministic arguments (`:65-67`) and
      `-p:CultLibPackageVersion`;
    - add its output to `$publishedByName` (`:131-140`);
    - add `"GameCult.Geometry.dll"` to `$expectedAssemblies` (`:104-130`). Do
      **not** add `CultMath.dll`: `org.gamecult.cultmath` ships it, and the copy
      loop (`:149-156`) copies only expected names;
    - after `:167-171`, add a check that the packaged `GameCult.Geometry.dll`
      version equals the package version, and that its referenced `CultMath`
      assembly version equals `packages/cultmath/unity/org.gamecult.cultmath/package.json`'s
      version (0.3.0 after 2a). This is the coupling rule: Aetheria must pin the
      cultmath tag Geometry was compiled against;
    - `-UpdateTemplate` (`:187-208`) already copies every expected assembly.
  - `unity/org.gamecult.cultlib/Runtime/GameCult.CultLib.asmdef`: add
    `"GameCult.Geometry.dll"` to `precompiledReferences`.
  - `unity/org.gamecult.cultlib/Runtime/Plugins/GameCult.Geometry.dll.meta` and
    `.pdb.meta`: new files. Copy `GameCult.Caching.dll.meta`'s import settings
    and give them a fresh guid.
  - `unity/org.gamecult.cultlib/package.json`: `1.1.0`.
  - `CHANGELOG.md`, `[1.1.0]` → Added: GameCult.Geometry (isosurface, surface
    nets, quad normals, geometry documents). It requires
    `org.gamecult.cultmath` 0.3.0.
  - Also note in the changelog that this release carries the 25 commits touching
    `unity/org.gamecult.cultlib`, `src/GameCult.{Mesh,Caching,Networking}` since
    `cultlib-unity-v1.0.60` (`git log cultlib-unity-v1.0.60..origin/main -- …`).
    Aetheria inherits them in Cut 7b.
- **Verification:**
  - Starfire, one job:
    `scripts/build-unity-package.ps1 -UpdateTemplate`, run detached, exit 0.
    Then a second non-incremental run leaves `git status` clean; this is the
    byte-reproducibility rule.
  - The version check must fail when fed a cultmath `package.json` at another
    version. Hands demonstrates this once, on a scratch copy, and reports it.
  - Unity: the Asura host project (Cut 4a) is not there yet. So a throwaway
    scratch Unity project pins `cultlib-unity-v1.1.0` (pushed to a scratch
    branch first, or a `file:` path) and `cultmath-unity-v0.3.0`, and calls
    `CultGeometrySurfaceNets.Extract` from an EditMode test. Zero compile errors.
  - Tags go at most three per push.
- **Hands budget:** ~150k tokens.

### Cut 4a. Asura: Unity package skeleton, host project, headless Core

- **Repo/branch:** Asura `main` `54a748a`. Hands commits code; Self commits the
  map.
- **Adds:**
  ```text
  unity/org.gamecult.asura/
    package.json                  name org.gamecult.asura, unity "6000.3"; no
                                  "dependencies": UPM package.json cannot declare
                                  git deps, so README states the required
                                  cultlib/cultmath tags
    README.md, CHANGELOG.md, LICENSE.md (match CultMath's MPL-2.0 unless the operator says otherwise)
    Runtime/Core/GameCult.Asura.Core.asmdef      noEngineReferences: true;
                                  precompiledReferences GameCult.Geometry.dll, CultMath.dll
    Runtime/Core/*.cs             AsuraBodyDefinition (Seed, Biome; no radius, D3/Q5),
                                  AsuraBiome (enum: Dunes, Craters|Rugged per Q4,
                                  plus analytic kinds Sphere, Box, CraterRim, Plane
                                  used as fixtures and as a legit baseline)
    Runtime/GameCult.Asura.asmdef references Core, CultMath.UnityBridge; engine refs
    Runtime/Shaders/              (empty until 4b)
    Tests/Editor/GameCult.Asura.Tests.Editor.asmdef   testables via host
  unity/AsuraHost/                Unity 6000.3.24f1, Built-in RP, Windows APIs D3D11 then D3D12
    Packages/manifest.json        "org.gamecult.asura": "file:../../org.gamecult.asura",
                                  cultlib-unity-v1.1.0, cultmath-unity-v0.3.0,
                                  com.unity.test-framework; "testables": ["org.gamecult.asura"]
    ProjectSettings/*             minimal; Library/, Temp/, Logs/, UserSettings/ git-ignored
  tests/Asura.Core.Tests/
    Asura.Core.Tests.csproj       net10.0; <Compile Include="../../unity/org.gamecult.asura/Runtime/Core/**/*.cs"/>;
                                  ProjectReference to CultLib GameCult.Geometry via a
                                  CultLibRoot sibling property (Aetheria.Shared's pattern,
                                  Aetheria.Shared.csproj:27-29), pinned to the cultlib-unity-v1.1.0 commit
    stryker-config.json           mutate **/unity/org.gamecult.asura/Runtime/Core/**/*.cs
  .gitignore                      Unity host dirs, bin/obj, StrykerOutput
  ```
- **Contract rules, headless (Core tests, Stryker):**
  - `AsuraBodyDefinition` equality and hash cover Seed and Biome. Two
    definitions that differ only in Seed are unequal.
  - The seed-to-noise-offset derivation (`AsuraSeed.Offset(seed)`) runs on the
    C# side and is uploaded, so the HLSL never re-derives it. It is
    deterministic, bounded (every component in `[0, 256)` so snoise keeps float
    precision), and distinct seeds give distinct offsets across a 10k-seed sweep.
    Use CultMath's `pcg3d`, not a local hash.
- **Verification:**
  - Yggdrasil: `dotnet test tests/Asura.Core.Tests` with a non-zero count, then
    Stryker.
  - Starfire: host batchmode EditMode run with one smoke test (the package
    compiles and Core types load). Log and PID in the scratchpad.
- **Hands budget:** ~150k tokens.

### Cut 4b. Asura: HLSL planet field, grid sampling, batched async readback

- **Adds:**
  - `Runtime/Shaders/AsuraField.hlsl`, the **one** implementation of the planet
    field (invariant 2):
    - It includes
      `Packages/org.gamecult.cultmath/Shaders/CultMath.hlsl`.
    - It exposes `float asura_field(float3 p, AsuraBodyGpu body)`, where `p` is
      in body-local unit space (D3). Inside is `<= 0`, matching surface nets.
    - `f = |p| − 1 − displacement(p)`, with `displacement` switching on the
      biome.
    - Dunes: large-scale banded ridges built from `cultmath_fbm` and ridged
      terms, with amplitude big enough to move the silhouette.
    - Craters: bowls and rims from `cultmath_cellular`, joined with `max` or
      `cultmath_smin` terms.
    - The analytic kinds.

    Each biome declares `maxAbsDisplacement`.
  - `AsuraField.hlsl` also exposes `asura_field_gradient(p, body, h)` as central
    differences. It serves Cut 6. CultMath has no analytic derivatives; if the
    benchmark shows the six extra evaluations are the cap, analytic snoise
    derivatives are a CultMath gap to fill there.
  - `Runtime/Shaders/AsuraSample.compute`: one kernel samples every pending
    body's grid into one flat `RWStructuredBuffer<float>`. A per-body table holds
    offset, dims, origin, cell size, and the seed offset and biome. The linear
    index within a body is `(x*ny + y)*nz + z`.
  - `Runtime/AsuraSampler.cs`:
    - Once per frame it packs the pending bodies up to a sample budget, makes one
      dispatch and one `AsyncGPUReadback.Request(buffer, size, offset, cb)`.
    - It keeps a ring of K sample buffers. A buffer is never re-dispatched while
      its readback is in flight; when all K are in flight, it skips the frame.
    - In the callback it copies each body's slice into a fresh `float[,,]` with
      one memcpy (`fixed (float* d = &grid[0,0,0])` and
      `UnsafeUtility.MemCpy`).
  - Grid bounds per body are the cube `±(1 + maxAbsDisplacement + margin)`, with
    margin ≥ 1 cell. Origin and cell size are derived so that the edge samples
    are strictly outside.
- **Mechanism probed in this pass:** a C# `float[,,]`'s memory is row-major.
  `Buffer.BlockCopy` of a flat `(x*ny+y)*nz+z` array into `float[2,3,4]` lands
  `[1,2,3] == 23`. A net10 scratch probe confirmed it, so the GPU layout above
  memcpys straight into the array `Extract` takes.
- **Authority map:**
  - Owner: `AsuraField.hlsl` owns shape.
  - Inputs: definition, level and seed offset.
  - Outputs: grid samples.
  - Derived: the `float[,,]` copy is a cache, dropped once meshed.
  - Forbidden writers: any C# evaluation of the planet field; anything writing
    the sample buffer except `AsuraSample.compute`.
- **Verification (Starfire, host EditMode tests on the real GPU, one job):**
  - Assert `SystemInfo.supportsAsyncGPUReadback` on the device. If it is false,
    Asura refuses loudly and keeps stand-ins.
  - **Layout:** the `Plane` kind (`f = a·p + b` with distinct a.x, a.y, a.z),
    sampled on a 5×7×9 grid and read back into `float[,,]`, equals the closed
    form at every index within float epsilon. This kills axis swaps and wrong
    strides.
  - **Ring integrity:** submit 4K bodies of the Plane kind with distinct
    coefficients over consecutive frames (PlayMode, `yield return null`). Every
    body's grid matches its own closed form, so no slice is overwritten in
    flight.
  - **Closure bound:** for each biome × 64 seeds × 3 levels, every boundary
    sample is `> 0`. This pins `maxAbsDisplacement`.
  - **Determinism:** the same definition sampled twice on this machine gives
    bit-equal grids. Invariant 6, same-machine; see Q7.
  - **Seed:** two seeds of the same biome give different grids.
  - negative: `rg -n "snoise|asura_field" unity/org.gamecult.asura -g "*.cs"` → 0.
    C# never evaluates the field.
- **Hands budget:** ~200k tokens.

### Cut 5a. Asura: worker-thread meshing and bare-mesh rendering

- **Adds:**
  - `Core/AsuraBuildJob`: a pure function taking `(float[,,], origin, cellSize)`
    to `CultGeometrySurfaceNets.Extract`, then
    `CultGeometryQuadNormals.FaceWeighted`, then an immutable result. There is
    one body per `Task`. Inputs are owned copies; there is no shared mutable
    state (invariant 7).
  - `Runtime/AsuraSystem` (MonoBehaviour), which owns the build lifecycle. Its
    port:
    - `Add(key, definition, Transform parent, Renderer standIn)`;
    - `Remove(key)`;
    - `SetCamera(Camera)`.

    Build generations are keyed by `(key, level, generation)`. A result whose
    generation is stale, or whose body was removed, is discarded on the main
    thread.
  - Mesh upload happens on the main thread: `Mesh.SetVertices(NativeArray)` and
    `SetIndices(..., MeshTopology.Quads)` with face-weighted normals, under a
    per-frame upload budget. It renders with Unity's built-in `Standard` shader.
    No new shader in this cut.
  - `Core/AsuraLevels`: projected pixel diameter to grid level, from a quantised
    table (for example 8, 12, 16, 24, 32, 48, 64). Downgrades have hysteresis.
    A level change remeshes, and the old mesh stays visible until the new one is
    ready.
  - **Stand-in switch.** Asura owns it, through the `standIn` renderer Aetheria
    passes in. For each body exactly one of the stand-in and the Asura mesh is
    visible at any frame after the first ready frame, and the stand-in is
    visible before that.
- **Contract rules:**
  - In Core: level selection is monotone non-decreasing in pixel diameter and
    clamped. Hysteresis means a diameter oscillating ±1% around a threshold never
    changes level.
  - In Core: builds of distinct bodies run in parallel and give results
    bit-equal to sequential builds.
  - In Unity: stale results are discarded.
  - In Unity: the stand-in exclusivity rule above.
- **Verification:**
  - Core tests and Stryker on Yggdrasil.
  - Host PlayMode on Starfire:
    - a Dunes body at 3 levels meshes and closes (every edge used by an even
      number of quads, and exactly 2 at the tested seeds) on the **real sampled
      grid**, not a CPU fixture;
    - removal mid-build leaves no mesh and no leaked `Mesh` (count `Mesh`
      objects before and after);
    - the stand-in/mesh exclusivity rule is checked every frame across a level
      change.
- **Hands budget:** ~200k tokens.

### Cut 5b. Asura: throughput benchmark with a committed baseline

- **Adds:**
  - A host PlayMode test with `[Category("Benchmark")]`, excluded from default
    runs. It frames through PlayMode, so async callbacks fire as in play.
  - For each grid size in {8, 12, 16, 32, 64} (at least 3 are required), it
    submits a burst of M distinct seeds and measures steady-state bodies per
    millisecond from first submit to last mesh upload, excluding warm-up.
  - It reports stage times: sample dispatch, readback latency in frames and ms,
    mesh ms per body on workers, and upload.
  - Mode "mesh-only (tiles pending)" is measured here; Cut 6b adds the tiled
    columns.
  - Output: `docs/benchmarks/<date>-<host>.md`, a table. Its header records GPU
    name and driver, CPU, Unity version, Asura, CultLib and CultMath SHAs, and
    "editor PlayMode, batchmode". Committed.
- **Verification:** the run on Starfire, one job, with no other heavy job
  running; the committed file. A player-build baseline is a recorded follow-up.
- **Hands budget:** ~120k tokens.

### Cut 6a. Asura: the tile compute pass at uniform N

- **Adds:**
  - `Core/AsuraPatchLayout`: from quad count, mesh edges and N, it gives the
    offsets of the three regions of D4:
    - `V` vertex points;
    - `E × (N−1)` edge points;
    - `Q × (N−1)²` interior points.

    It also derives mesh edges from the quad mesh, keyed `(min vi, max vi)`.
    Edge points are parameterised from the lower vertex index to the higher, so
    the parameterisation belongs to the edge and not to either quad.
  - `Runtime/Shaders/AsuraTiles.compute`, with kernels for vertex, edge and
    interior points. Each point:
    - starts from its canonical position (the base vertex; lerp of the two
      refined endpoints; bilinear of the four refined corners);
    - is refined onto `f = 0` by normal-constrained Newton: a fixed iteration
      count, moving along the gradient direction at the start point, with the
      step clamped to one base cell;
    - stores the gradient normal and a biome material (packed RGBA8).

    Vertex points are refined first, so edge and interior start points read
    refined corners.
  - A per-body `GraphicsBuffer` of `AsuraPatchPoint {float3 position; float3
    normal; uint material}` (D5).
  - `Core/AsuraLevels` gains N selection from projected base-cell pixels (for
    example N in {1, 2, 4, 8, 16}), with the same monotone, clamp and hysteresis
    rules.
  - Core's remesh/retile decision: an N change retiles only, and a level change
    remeshes and then retiles.
- **Authority map:**
  - Owner: the tile kernels own tile contents.
  - Inputs: field, quad mesh, N.
  - Outputs: the patch buffer.
  - Derived: everything in it is a cache (invariant 1).
  - Forbidden writers: anything else writing the patch buffer; anything reading
    positions back as shape authority. The dock-site query in 7c evaluates the
    field, not the atlas.
- **Verification (Starfire, host tests reading the buffer back):**
  - **Refinement:** on the Sphere kind at N ∈ {1, 2, 4, 8}, every point satisfies
    `| |p| − 1 | ≤ ε`. At N = 1 this also shows base vertices snapped onto the
    surface.
  - **Normals:** on the Sphere, `dot(normal, normalize(p)) ≥ 1 − 1e-3` at every
    point.
  - **Dunes:** `|f(p)| ≤ ε·cell` for every point, and no point moves more than
    one base cell.
  - Core tests and Stryker for the layout offsets and edge derivation (each
    unique edge appears once; key order is canonical).
- **Hands budget:** ~200k tokens.

### Cut 6b. Asura: patch rendering and watertightness at uniform N

- **Adds:**
  - `Runtime/Shaders/AsuraPatch.hlsl`. It holds the one function
    `asura_patch_vertex(vertexId) → (point index, patch uv)`, which expands each
    quad into an N×N patch of `2·N²` triangles, reading corners, edges and
    interior from the D4 regions.
  - `AsuraPatch.shader` (Built-in RP): the vertex shader calls that function and
    reads the patch buffer; `ForwardBase` lighting uses the stored normal and
    material. It draws with `Graphics.RenderPrimitives` (procedural, no mesh),
    one draw per body. Shadows are out of this cut.
  - `AsuraPatchExpand.compute`: a verification kernel that calls the **same**
    `asura_patch_vertex` and writes the expanded triangle positions into a
    buffer. It exists so watertightness is measured on exactly what the vertex
    shader emits, with no C# replica of the expansion.
  - Presentation: a body whose tiles are ready draws patches. Before that, the
    bare mesh (5a) is drawn with face-weighted normals as the stand-in. One of
    the two is visible at a time, per body.
  - The benchmark (5b) gains columns: tiled at N ∈ {1, 2, 4, 8}, per grid size.
    A new committed results file is written; the old one is kept.
- **Verification (Starfire):**
  - **Watertight:** for Sphere, Dunes and Craters at N ∈ {1, 2, 4, 8} × 2 levels,
    read back `AsuraPatchExpand`. Each undirected triangle edge, keyed by the
    **exact bit patterns** of its two endpoints, appears an even number of times,
    and exactly twice on the Sphere. Report per case the edge count and the
    number of odd edges (must be 0).
  - **No folds:** each triangle's normal has a positive dot product with the
    field gradient at its centroid.
  - **Exclusivity:** the bare-mesh/patch switch never shows both and never
    neither, across a retile.
  - operator: the dunes planet in the host scene, orbiting camera: the
    silhouette shows the dunes.
- **Hands budget:** ~180k tokens.

### Cut 6c. Asura: crease snapping from field gradients in the tile pass

- **Adds**, in the tile kernels, after refinement, for every point (vertex points
  included, so N = 1 gets sharp base vertices and the mesher stays pure surface
  nets):
  - It samples `asura_field_gradient` on a stencil of fixed offsets in
    **body-local space**. The offsets depend only on (body, grid level): never
    on the quad, N or patch coordinates.
  - It clusters the gradients into families by an angle threshold.
  - With 2 or more families, it solves a small QEF over the tangent planes
    (two planes give the nearest point on the crease line, three give the
    corner), regularised toward the refined point.
  - It clamps the move to a fraction α of the patch cell (base cell ÷ N) so no
    triangle folds.
  - It sets a "snapped" bit in the point's material word. This is the
    observation at the layer where the rule is decided, and it stays in
    production.

  Prior art is cited in a comment block and in `docs/`: Kobbelt et al. 2001
  (Extended Marching Cubes, feature-sensitive sampling) and Ju et al. 2002
  (dual contouring QEF). The implementation is fresh.
- **Do not add:** a dual-contouring placement option in CultLib (ruling,
  `target.md`, "Deferred paths").
- **Verification (Starfire, readback):**
  - **Box** kind at N ∈ {2, 4, 8}: every point within one base cell of a box
    edge lies within ε of the true edge line; points near corners lie on the
    corner; zero folded triangles (6b's fold check); watertight (6b's bit-edge
    check).
  - **Box at N = 1:** base vertices near box corners land within ε of the
    corners.
  - **Sphere control:** the snapped-bit count is 0, and the patch buffer is
    bit-equal to the output with snapping compiled out. The rule is "never
    triggers", not merely "small effect".
  - **CraterRim** kind (a max of terms): points within one cell of the analytic
    rim circle lie within ε of it; zero folds; watertight.
  - The benchmark gets one more run to record snapping's cost.
- **Hands budget:** ~180k tokens.

### Cut R. Asura release tag

- `asura-unity-v0.1.0` on the commit that closes 6c. Package version `0.1.0` and
  CHANGELOG. The README states the required cultlib and cultmath tags. Soul
  passes before the tag is pushed.

### Cut 7a. Aetheria: planet definitions from zone generation (headless)

- **Repo/branch:** Aetheria, `hands/asura-7a`, based per Q1. Worktree.
- **Per-file changes (master `9b85211f`):**
  - `Assets/Scripts/ServerShared/ZoneData.cs:69-73`: `PlanetData` gains
    `[Key(9)] uint Seed` and `[Key(10)] string Biome`, using the Asura biome
    name. It is a string so ServerShared never references Asura; see the
    negative check below. The document version follows Aetheria's
    schema-evolution rule for `aetheria.planetdata`, and Hands reads that rule
    before choosing. Old records per Q3.
  - `Assets/Scripts/ServerShared/ZoneGenerator.cs:125-128` (the Planet/Planetoid
    branch):
    - Seed comes from a **local** generator built from the zone's stable hash
      and the planet's index in `planets`. The zone generator already hashes
      `galaxyZone.Name.StableHash() ^ pcg3d(position).x` at `:48`.
    - It never draws from the shared `random` stream. Drawing from it would shift
      every later draw at `:143-196` and `:209+` (belts, gas colours, stations),
      so existing galaxies would regenerate differently.
    - This is the same pattern as fire-control 6b's `CombatSeed` plus a local
      generator (`Zone.cs` on `ab14552a`).
    - Biome comes from the seed per Q3.
- **Contract rules (`tests/Aetheria.Shared.Tests`, Stryker via the fire-control
  line's `stryker-config.json`):**
  - The same galaxy zone gives the same seeds and biomes.
  - Distinct planets in one zone get distinct seeds.
  - **Every other field of the generated `ZonePack` and its bodies is unchanged
    against a capture taken at the base commit.** Capture it at the base, not
    from the new code.
- **Negative:** `rg -n "Asura" Assets/Scripts/ServerShared` → 0. `Aetheria.Shared`
  compiles ServerShared headless without the Asura package, so any reference
  fails that build. This is invariant 7 made structural: zone gen cannot build
  geometry.
- **Hands budget:** ~120k tokens.

### Cut 7b. Aetheria: Asura renders rocky bodies

- **Per-file changes (master anchors; fire-control anchors differ by the
  `:292-301` EngineAssets edits only):**
  - `Packages/manifest.json:54-55`: `org.gamecult.cultlib` to
    `cultlib-unity-v1.1.0` and `org.gamecult.cultmath` to `cultmath-unity-v0.3.0`
    (Cut 3's coupling rule), and add
    `"org.gamecult.asura": "https://github.com/GameCult/Asura.git?path=/unity/org.gamecult.asura#asura-unity-v0.1.0"`.
    Unity regenerates `packages-lock.json`; commit it.
  - `ZoneRenderer.cs`:
    - a serialized `AsuraSystem Asura` field next to `:40`, and an `AsuraSystem`
      object in `ARPG.unity` wired to it;
    - `SetCamera(MainCamera)` in `Start` (`:174`).
  - `ZoneRenderer.cs:394-403`, the rocky branch: after `Instantiate(Planet)`,
    call `Asura.Add(key, def(PlanetData.Seed, Biome), planet.Body.transform,
    planet.Body)`.
    - `planet.Body` is the `Terrain Mesh` renderer: `high-res-sphere.fbx` with
      `Planet.mat`. It is the stand-in; see the finding on the target's "flat
      quad".
    - The Asura child transform's local scale equals the stand-in mesh's
      `bounds.extents.x`, so the swap keeps apparent size (D3).
  - `ZoneRenderer.cs:271-276` (`ClearZone`): `Asura.Remove(key)` for each rocky
    key before `DestroyImmediate`.
  - `:497` rotates `Body`. The Asura child inherits the rotation, so nothing
    changes here.
- **Verification (Starfire, Unity closed, one job):**
  - batchmode compile;
  - the Aetheria EditMode suite;
  - a PlayMode smoke: load a zone and assert that every rocky body reaches ready
    within T seconds, and that exclusivity holds;
  - on the fire-control line, `EngineAssetCheck.Run` exits 0;
  - operator: enter zones and see rocky planets as dunes or craters, varying by
    seed and stable across reloads. Gas giants and suns are unchanged.
- **Hands budget:** ~180k tokens.

### Cut 7c. Docked view: dock-site query and raised N with per-edge stitching

- **Blocked on Q6.**
- **Adds (Asura):**
  - `AsuraQuery.compute`, which includes `AsuraField.hlsl`. For a body and a
    direction it finds the outermost `f = 0` crossing, marching inward from the
    bound and then bisecting, and returns the point and the gradient normal.
    Readback is async. This is the target's field query; it never reads the atlas.
  - `SetFocus(key, direction, radius)`: quads whose centres lie within the focus
    radius get `N_hi`, and the rest get the body's N.
  - Per-edge factor, keyed by the mesh edge (D4): `E(edge) = max(N_a, N_b)`.
  - The edge region holds `E − 1` points per edge. `asura_patch_vertex` stitches
    each quad's boundary rows to its edges' factors.
  - The layout moves from uniform slots to per-quad offsets, computed in Core.
- **Adds (Aetheria), per Q6:**
  - The dock-site direction is derived from the station's identity; Aetheria
    owns it.
  - `ActionGameManager.cs:863-881` (`DoDock`): when the parent orbit body is a
    rocky `PlanetData`, frame the close-up. The station asset is placed at the
    query point and aligned to the normal, and `SetFocus` is called. Otherwise
    the current dock camera path applies unchanged. `Undock` (`:883+`) clears
    the focus.
- **Verification:**
  - Starfire:
    - on the Sphere kind, the query returns `R·dir ± ε`, with normal = dir;
    - watertight: 6b's bit-edge check at **mixed** N (focus on, `N_hi`
      ∈ {4, 8, 16} beside N ∈ {1, 2}), zero odd edges and zero folds;
    - Core Stryker on the per-edge factor and offsets.
  - operator: dock at a station orbiting a rocky planet. The station sits on the
    surface without floating or sinking, and there is no crack at the N
    boundary.
- **Hands budget:** ~200k tokens. If Q6 adds the flattened pad (a `cultmath_smin`
  of a plane into the field), split it into 7d.

## Subtraction ledger (estimates)

| Cut | Removed | Added | Deps/targets |
| --- | --- | --- | --- |
| 1 | 310 files, 11,344 text lines, 16 LFS binaries, ~60 C#/YAML lines (+3 files per Q2) | 0 | −1 third-party plugin |
| 2a | 0 | ~90 C#, ~70 HLSL, ~150 test | cultmath-unity 0.3.0 |
| 2 | 0 | ~220 C#, ~300 test, 1 md, 1 Stryker config | none |
| 3 | 0 | ~20 script, 1 asmdef line, 2 metas, 1 DLL+pdb | cultlib-unity 1.1.0 |
| 4a | 0 | package skeleton, host project, ~150 C#, 1 test csproj | +1 Unity package, +1 host project, +1 test project |
| 4b | 0 | ~150 HLSL, ~200 C#, ~250 test | none |
| 5a/5b | 0 | ~350 C#, ~250 test, 1 results doc | none |
| 6a/6b/6c | 0 | ~450 HLSL, ~250 C#, ~400 test, 2 results docs | none |
| 7a/7b/7c | ~5 lines | ~150 Aetheria C#, ~250 Asura, ~200 test | Aetheria +1 UPM dep (asura) |

This is net additive, and deliberately so: the campaign buys a capability that
did not exist (Asura). Cut 1 pays about 11k lines of unused third-party plugin
toward it.

## Operator questions

- **Q1. Which Aetheria line do Cuts 1 and 7 land on?**
  - A: wait until the fire-control line (`ab14552a`, 218 commits ahead of
    master) merges to master, then branch from master.
  - B: branch from master `9b85211f` now.
  - C: stack on the fire-control line.

  **Recommended: A.** Cuts 2–6 need nothing from Aetheria, so waiting costs
  nothing on the critical path. B would conflict with settings Cut 0's
  `GameSettings.cs` edits and with the manifest pins. C couples Asura to
  unreviewed in-flight work.
- **Q2. `PlanetOutpost.prefab` and `Locations/ReconStationAlpha.{prefab,asset}`
  use the plugin.** Nothing but the content catalog (not yet decoded) could
  reference them.
  - A: delete them in Cut 1, if the decode finds no consumer.
  - B: keep them, stripping the plugin component and material. Their bodies then
    render with no material.

  **Recommended: A.** It is the subtraction cut, and PlanetOutpost's concept (a
  station on a planet) is what 7c builds properly. If the decode finds a
  consumer, Hands stops.
- **Q3. Seeds, biomes and old saves.**
  - (a) Seed from a local generator (zone hash and planet index) that never
    consumes the zone stream. **Recommended**, as above.
  - (b) Biome: uniform by seed over Asura's biome set now, or banded by mass
    (planetoid vs planet), using the existing
    `BodyType.Planetoid`/`Planet` split at `ZoneGenerator.cs:117`?
    **Recommended: uniform now,** with a mass band added later as a
    `ZoneGenerationSettings` entry.
  - (c) Stored `PlanetData` without a Seed, in existing runs: should they
    (A) fail loudly and be regenerated, or (B) derive a seed from the record key
    on load? **Recommended: A.** B is a compatibility path that keeps deciding
    shape forever. Is there any saved run you need to keep?
- **Q4. The second minimal biome.**
  - A: craters. This needs `cultmath_cellular` in 2a, and craters are the
    asteroid look.
  - B: rugged fBm. It needs no extra CultMath.

  **Recommended: A.** Asteroids are in scope, and the crater rim is the crease
  case 6c must prove anyway.
- **Q5. Do biome features scale with the body?** Should a small rock and a large
  planet of the same biome look like scaled copies (unit-radius field, D3), or
  keep a world-space feature size (the definition carries a radius)?
  **Recommended: scale with the body.** It is the tiny-planet style, and it
  keeps float precision uniform. The target's "(seed, radius, biome
  parameters)" then loses radius from the Asura definition, and the world
  radius stays Aetheria's `BodyRadius`.
- **Q6. What is the docked view?** Stations are generated orbital entities at
  Lagrange points (`ZoneGenerator.cs:208-215`). `DoDock` today follows the
  station and looks at its parent planet (`ActionGameManager.cs:863-881`).
  - A: presentation only. When the station's parent is a rocky planet, the
    docked camera shows the station asset standing on that planet at a dock
    direction derived from the station's key. Simulation positions are
    unchanged. Stations around gas giants or suns, or with no parent, keep
    today's view.
  - B: authored dock sites per station.

  **Recommended: A.** Also: should the flattened pad under the station ship in
  7c, or wait for clipping to show it is needed? **Recommended: wait.**
- **Q7. Invariant 6, "on any machine".** CultMath's HLSL parity is proved on the
  CPU: the text of the mirror is compiled as C#. Its own test comment says it
  "does not prove GPU agreement". Because the field is GPU-only, grids can
  differ in low bits across GPUs and drivers, and a sample near 0 can flip a
  quad.
  - A: reword to "bitwise on one machine; across machines within tolerance.
    Nothing persisted or networked depends on shape bits."
  - B: require cross-machine bit equality. That would need a CPU field, which
    contradicts F1 path A.

  **Recommended: A.**
- **Q8. Belt asteroids.** Belts are thousands of instanced meshes from
  `LowPoly_AsteroidsPack` (`ZoneRenderer.cs:339-363`).
  - A: this campaign covers `PlanetData` bodies (planets and planetoids) only.
    Belts get a later cut sized by the 5b/6b benchmark, for example a pool of K
    Asura rocks per belt, instanced.
  - B: include belts in Cut 7.

  **Recommended: A.** The Jevons ruling makes the benchmark the right gate.
- **Q9. Licence of the Asura package.** CultMath is MPL-2.0 and Aetheria source
  is MPL-2.0; the cultlib package is MIT. **Recommended: MPL-2.0,** matching
  Aetheria and CultMath.

## Findings for Self (target reconciliation; not assigned to a cut)

- **The target's "flat body/icon/gravity-well quads" is half wrong.** `Body` is
  the `Terrain Mesh` object: `Assets/Models/high-res-sphere.fbx` with
  `Assets/Materials/Planet.mat` (GlowFade shader). Only `Minimap Icon` (layer 14)
  and `Gravity Well` are quads (`Planet.prefab:15,43,98,126,254,282`). The
  main-view stand-in is that sphere, not "the flat icon".
- **The target's "no scene instantiates it" is half wrong.**
  - `Planet.prefab` carries a disabled generator.
  - `PlanetOutpost.prefab` carries an **enabled** one.
  - `ARPG.unity` runs an enabled `LODHandler`.
  - `ZoneRenderer.cs:400` assigns random plugin settings per planet with
    `UnityEngine.Random`.

  The plugin's meshes never generate in play, because the planet generator is
  disabled, but it is wired in.
- **The target's substrate pins** (cultlib 1.0.60, cultmath 0.2.4) are the
  fire-control line's. Master pins 1.0.59 and 0.2.3
  (`Packages/manifest.json:54-55`).
- **An unmerged prior planetary line exists in CultLib:** tag
  `gamecult-geometry-unity-v0.1.0` and `origin/codex/geometry-*`, dated
  2026-07-22, with a cube-sphere heightfield, erosion, GPU pages and an
  `org.gamecult.geometry` Unity package. Its progress doc stops at "Stage 6
  consumer cutover … Gate pending". Main's `GameCult.Geometry` went another way
  (isosurface, 2026-08-26). Add it to the target's "Not consumers", so no pass
  mistakes it for Asura's substrate. Its fate belongs to whoever owns the
  geometry-ownership migration, not to this campaign.
- **The target mentions smooth-min bevels and pads,** but CultMath has no smin.
  Cut 2a fills that gap.
- **Identity table:** propose a row for "mesh edge (vertex-index pair ≙ grid
  face) → edge point block, per-edge factor" (D4 and 7c). Its lifecycle matches
  the tile slot's, its owner is the Asura tile pass, and it is regenerable.
- **Aetheria's `settings-globals-cut.md` fork B** (`:527-535`) plans to move the
  plugin's 7 body-settings assets into `Resources`. F2 supersedes that; sweep it.
- **`ZoneRenderer.cs:497`** rotates `Body` by `PlanetRotationSpeed` per frame,
  not per second, so planet spin depends on frame rate. It is not Asura's
  concern; record it as an Aetheria follow-up.
- **Missing substrate:** the voidbot MCP was unreachable (connection timeout)
  for this pass, so everything here comes from direct git reads and probes.
