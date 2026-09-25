# Asura: cut map

Status: cut map, Imagination pass 2, 2026-09-25. Cut 2 is being executed by
Hands in `F:\Projects\CultLib-asura-surface-nets`. Nothing has landed yet.
`docs/target.md` owns the ends. This document owns the means. Where this map and
the Body disagree, the Body wins and this map is stale. The map lives on `main`
of `GameCult/Asura`.

Pinned HEADs (every `file:line` below is against these):

| Repo | Ref | SHA | Note |
| --- | --- | --- | --- |
| Asura | `main` | `5f0f6bd` | target with the rulings on Q1–Q9, archetypes, the erosion filter and invariant 8 |
| CultLib | `origin/main` | `29e50ad` | Local `main` in `F:\Projects\CultLib` is 15 commits behind (`d110d9d`). The main checkout sits on `hands/cultmath-erf`. Work from `origin/main` in a worktree. |
| CultLib tags | `cultmath-unity-v0.2.4` / `cultlib-unity-v1.0.60` | `6d5e209` / `45c2f40` | both are ancestors of `29e50ad` |
| Aetheria | `origin/master` | `9b85211f` | mainline, 2026-09-17; pins `cultlib-unity-v1.0.59` and `cultmath-unity-v0.2.3` |
| Aetheria | `origin/codex/fire-control-12` | `ab14552a` | In flight: 218 commits ahead of master and 2 ahead of the local checkout (`14ee2a52`). It pins cultlib `1.0.60`, cultmath `0.2.4` and caching `1.4.0`. **Do not touch `F:\Projects\Aetheria`'s tree.** |

Rulings: the target's dated rulings, including "Rulings on the cut map's
questions (2026-09-25)". Summary:
- Q1: Aetheria cuts go on master, after fire-control merges.
- Q2: delete the outpost and recon assets.
- Q3: seed from a local generator, archetype by seed, no compatibility path.
- Q4: bodies are archetypes, material (rock | ice) × activity (dead | active),
  with hydraulic erosion switched on per body, using Johansen's erosion filter.
  Phacelle noise and simplex noise with an analytic gradient go in CultMath;
  the filter goes in Asura.
- Q5–Q9: as recommended.

The answered questions are recorded under "Operator questions".

Later operator additions the same day:
- aeolian dunes: "1-abs(snoise(p)) gives nice dune contours", used "to push the
  terrain really far at really low frequency to influence the silhouette";
- invariant 8: "make sure we have analytic derivatives flowing down every
  level".

Progress (Self keeps this current):
- **Cut 2:** Hands landed it on CultLib `hands/asura-surface-nets`
  (`29e50ad..659e3dd`, 6 commits, all building). Surface-nets tests pass.
  Stryker (`--since`, which fell back to the whole glob on this first config)
  left 20 survivors: in the cut's own files, one float-threshold flip and three
  claimed vstest false survivors; the rest are in untouched files.
  - Reported by Hands as pre-existing: 7 `GeometryDocumentTests` fail at
    `29e50ad` (`CultGeometryBuildRequest.DomainKey` reference not walkable by
    `CultDocumentRegistry.Refresh`). Soul is diagnosing the cause and owner.
  - Soul pass 1 found real defects:
    - F1: Stryker never mutated `Extract`, because unassigned locals turned all
      98 mutants into compile errors.
    - F4: orientation had two authorities, which split on degenerate quads.
    - F3: the float guard made answers worse.
    - F2: the survivor triage was wrong.
    - F6, F7, F8: minor.

    Fix batch 1 is out to a fresh Hands.
  - The 7 `GeometryDocumentsTests` failures: CultLib `e420410` (CultNet
    selection, cut 1, commit 0) added registry check D11, which refuses
    `[CultReference]` on `string` members. Geometry has three of them
    (`CultGeometryDocuments.cs:156, :231, :316`). The tests passed at `b3d9cf7`
    and fail at `e420410`. **This blocks Cut 3**, because Unity consumers would
    throw on registering Geometry documents. It is an operator question (Q13).
  - Follow-ups outside Asura: `CultGeometryIsoSurface.cs:148` has the same
    worse-than-exact float guard, and IsoSurface also accepts non-finite samples.
    The GameCult.Geometry owner should mirror Cut 2's fixes there.
- **Cut 2a-i:** Hands landed it on CultLib `hands/cultmath-asura-noise`
  (`29e50ad..1add078`, 8 commits, all building).
  - 184 of 184 CultMath tests pass. FXC `cs_5_0` compiles a kernel using both
    new functions. dxc was not run: the tool is missing on Starfire.
  - Soul pass 1 is running. Suspects: the tolerance change in `08b0dce`, the
    deleted F2 guard, and whether a 3×3×3 search is complete for F2.
  - **Budget scar:** Hands ran to about 340k tokens against an estimate of
    ~150k. Split later CultMath cuts finer (e.g. one primitive family per
    Hands).

Answered 2026-09-25 (see target, "Rulings on the second map pass"):
- Q10: independent 0.25 draws in zone-gen settings. Galaxy-driven variance is a
  later campaign.
- Q11: the band-limit wording is accepted and is in target invariant 8.
- Q12: A. Erosion keeps upstream's analytic derivative, checked against a
  committed error bound. There are no second derivatives this campaign.

Nothing is open. Cuts 1 and 7 wait on fire-control merging to Aetheria master.
The Q10–Q12 texts at the end are kept as history; these answers supersede them.

## Cut order

```text
2a-i CultMath basics ─> 2a-ii gradient noise + Phacelle ─> cultmath 0.3.0 ─┐
2    surface nets (in flight) ──────────────────────────────────────────────┴─> 3 CultLib Unity release
  ─> 4a Asura skeleton ─> 4b-i field + sampling + Moon ─> 4b-ii lineae, dunes, plains ─> 4b-iii erosion (after Q12)
  ─> 5a mesh + bare render ─> 5b benchmark
  ─> 6a tile pass ─> 6b patch render + watertight ─> 6c crease snap
  ─> R Asura release tag
1 delete plugin (Aetheria master, after fire-control merges) ─> 7a defs ─> 7b integration ─> 7c docked view
```

4b-ii and 4b-iii can run beside 5a in separate worktrees. 5a needs only 4b-i's
grids, and the later terms are more terms in the same field.

Re-splits from the brief, with reasons:
- **2a is split in two**, 2a-i and 2a-ii, with one release at the end. After
  Q4, 2a carries five primitives, and they no longer fit one Hands pass. See
  2a for the reasons.
  - CultMath has no fBm, smooth-min, cellular noise, gradient noise or Phacelle
    noise. The HLSL function list at `29e50ad` is catmullrom, clamp, csum, the
    béziers, damp, decay, degrees, distance, frac, hash, lengthsq, lerp,
    pcg/pcg3d/pcg4d, reflect, rotate, saturate, the smoothsteps, snoise, step and
    value_noise*.
  - Gaps are filled in the owner.
  - 2a releases before Cut 3, because Cut 3's Geometry DLL is compiled against
    the CultMath version it ships beside (see Cut 3).
- **Cut 4 is split** into the skeleton (package, host project, headless Core
  tests) and the GPU work. The GPU work is split three ways:
  - 4b-i: plumbing and the first archetype;
  - 4b-ii: the remaining terms;
  - 4b-iii: the erosion port with its sphere adaptation.

  Each part is roughly 150–200k tokens.
- **Cut 5 is split** into the build path and the benchmark. The benchmark has
  its own harness and its own committed artefact.
- **Cut 6 is split three ways**, as the operator asked: the compute pass,
  then rendering with a watertightness proof at uniform N, then crease snapping.
- **R (Asura release tag)** is its own cut, because a tag is a cut and gets its
  Soul pass before it is pushed (SKILL.md step 5).
- **Cut 7 is split** into definitions (headless, mutation-reachable), then
  render integration, then the docked view.
- **Cut 1** runs on Aetheria master once fire-control has merged (Q1). Nothing in
  Cuts 2–6 depends on it.

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
  (`ZoneRenderer.cs:406` at master). Q5 ruled for this on 2026-09-25. The field
  is `f(p) = |p| − (1 + h(p/|p|))`, where `h` is a height over the unit sphere.
  Inside is `f <= 0`.
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

  **Edge use is even, not always 2** (Soul, Cut 2 pass 1). Where the grid
  under-resolves the field, surface nets emits edges used by 4 quads, and it
  does so on realistic terrain, not only checkerboards. Soul measured 12 on a
  24³ blob at amplitude 1.2, 93 at amplitude 3.0, and 11 on a 64³
  low-frequency field. The key is still the vertex pair, so all 4 quads read
  the same edge memory and the surface stays watertight. Nothing may assume 2:
  - no "the other quad" lookup;
  - no manifold-only adjacency;
  - no per-edge storage sized as quad-count × 2.

  Cut 6b's watertightness proof must include an under-resolved noisy field with
  4-use edges. Band limits (D9) reduce how often this happens but do not
  guarantee it never does.
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
- **D8. Analytic derivatives at every level (target invariant 8).** The
  operator, 2026-09-25: "make sure we have analytic derivatives flowing down
  every level." `asura_field(p, body, band) → float4(∇f.xyz, f.w)`.
  - Every term and every CultMath primitive under it returns value and gradient
    together, in the same layout, and composition uses the chain rule.
  - The sphere composition is `∇f = u − (I − uuᵀ)∇h/|p|`, with `u = p/|p|`.
    **This is the highest-risk site for a quiet error:** a sign, a dropped
    projection, or a missing `1/|p|`. 4b-i tests it off the unit sphere.
  - There are **no finite differences anywhere in the field or its
    consumers.** Sampling, Newton refinement, tile normals, crease detection,
    the erosion input and the dock query all read the one analytic gradient.
  - One evaluation per point then serves everything. Central differences would
    have cost 4–7 per point.
  - Finite differences exist only in tests, as the property-test oracle.
  - The one term that cannot meet a finite-difference tolerance is erosion
    (probe: 4b-iii). Q12 puts that fork to the operator.
  - Second derivatives (the erosion article's curvature feature) are out of this
    campaign; record them.
- **D9. Every multi-octave term is band-limited by the caller's footprint.**
  The consumer passes its sample spacing:
  - the grid cell size for sampling;
  - the patch spacing (base cell ÷ N) for tiles;
  - a fixed small footprint for the dock query.

  Octaves above Nyquist for that spacing are dropped, and the last kept octave
  is faded. This applies to fBm, ridged dunes, crater scales, lineae scales and
  erosion octaves. Without it, the coarse grid aliases erosion gullies and dune
  crests into topological noise in the surface-net mesh. The full field is the
  footprint → 0 limit. Each lowering evaluates the same function, band-limited to
  what it can resolve, so tile refinement adds exactly the detail the grid could
  not hold. Watertightness is unaffected: D4 stores shared points once, and a
  body's patch spacing is uniform except in 7c, where edges carry their own
  factor. Target amendment for Self: Q11.

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

- **Repo/branch:** Aetheria, `hands/asura-cut1`, from `master` **after the
  fire-control line has merged** (Q1, 2026-09-25). Before dispatch, Self re-pins
  master's HEAD and re-checks the anchors below. They are given for master
  `9b85211f` and for `ab14552a`, because post-merge master will carry
  `ab14552a`'s lines. The plugin's own files are identical in both.
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
  - **Q2, ruled delete (2026-09-25):**
    `Assets/Resources/Prefabs/Stations/PlanetOutpost.prefab` (+meta) and
    `Assets/Resources/Locations/ReconStationAlpha.{prefab,asset}` (+metas).
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
  - catalog: `EngineAssetCheck.Run` in batchmode exits 0 after the delete. It
    arrives with fire-control (`docs/addressables-cut.md:220` on `ab14552a`). If
    post-merge master somehow lacks it, fall back to a scratch decode of
    `GameData/Aetheria.cc` through AetherDb's cache-open path. That decode must
    find 0 string fields containing `PlanetOutpost`, `ReconStationAlpha` or
    `Celestial Body`, and print the count of strings scanned; the probe stays
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
  −~15 C# lines, −~45 YAML lines, −2 prefabs and −1 asset (Q2).
- **Hands budget:** ~120k tokens.
- **Doc sweep for Self:** `docs/settings-globals-cut.md` fork B (`:527-535`),
  which moves the 7 body-settings assets into `Resources`, is superseded by F2.
  Mark it history in that map the day this cut lands.

### Cut 2a. CultMath: the noise primitives Asura's field needs (2a-i, 2a-ii)

- **Repo/branch:** CultLib, `hands/cultmath-asura-noise`, from `origin/main`
  `29e50ad`, in its own worktree. Do not use Cut 2's
  `F:\Projects\CultLib-asura-surface-nets`. 2a-ii continues on the same branch
  after 2a-i lands. One release, `cultmath-unity-v0.3.0`, at the end of 2a-ii:
  it is additive, so a minor bump per `docs/semver-policy.md`, and it gets a Soul
  pass before the tag. There is one release rather than two because every
  CultMath release forces a matching `org.gamecult.cultlib` release (Cut 3's
  coupling rule).
- **Common rules for every addition:**
  - It lives in C# `math` (`packages/cultmath/src/CultMath/math.cs`, near
    `snoise` at `:594`) and in `shaders/CultMath.hlsl`, with the same name
    prefixed `cultmath_`.
  - Both tracked `CultMath.hlsl` copies stay the same blob (`6179d45` today).
  - Bodies stay inside the transformation list at
    `packages/cultmath/docs/design.md:92`.
  - **Invariant 8 (operator, 2026-09-25: "make sure we have analytic
    derivatives flowing down every level").** Every new primitive returns its
    value and its analytic gradient together, laid out as
    `float4(∇.xyz, value.w)`:
    - `r.xyz` is directly the gradient as a `float3`, and `r.w` is the value;
    - this is the same layout for every primitive;
    - new primitives have **only** this form. A value-only twin would be a
      second path with no consumer, since the value is `.w`;
    - the existing value-only `snoise(float3)` and `snoise(float2)` stay, because
      they are public API and removing them is a semver major.
  - Two primitives yield more than one value and gradient: cellular (the F1 and
    the edge distances, plus the nearest cell id) and Phacelle (cos and sin, each
    with a gradient). They return a small CultMath struct instead of recomputing
    the neighbourhood search per output: `CultCellular` and `CultPhasor`.
    - Today the mirror comparison reflects over C# counterparts with matching
      parameter types, and compares only scalar and vector returns.
    - So 2a-i extends `design.md`'s transformation list (`:92`) and
      `HlslSourceCompatibilityTests` to allow one struct return shape, compared
      field by field and bit for bit.
    - That extension happens once, in the owner, with its own test: a struct
      mirror whose field differs must fail.
  - Cell selection hashes with `pcg3d`, which is integer-exact on CPU and GPU
    (`design.md`, "Integer hashing"). Nothing new uses the `sin`-based `hash`.
  - Names must not contain `spherical_erosion` or pull in
    `AdvancedErosionFilter.hlsl`. `HlslMirrorTests.cs:7-15` forbids erosion
    kernels in CultMath, and that ruling stands: the erosion filter is Asura's
    (4b-ii).
  - `THIRD-PARTY-NOTICES.md` (both the `packages/cultmath` and the unity package
    copies) gains an entry for every ported primitive.

#### 2a-i. Smooth-min, cellular, and the mirror-test struct extension

- **Adds:**
  - `smin_grad(float4 a, float4 b, float k) → float4`: polynomial smooth
    minimum of two value-and-gradient forms. The output gradient is the blended
    gradient, `lerp(∇b, ∇a, h)` with the same blend factor `h` as the value. It
    serves the target's bevels, the pad blend, crater rims and flood plains.
  - `cellular(float3 p) → CultCellular { float4 nearest; float4 edge; float id; }`:
    - `nearest = (∇F1, F1)`, with `∇F1 = (p − c1)/F1`, the unit vector from the
      nearest feature;
    - `edge = (∇F2 − ∇F1, F2 − F1)`, for lineae along cell borders;
    - `id` is the nearest cell's `pcg3d` hash mapped to [0,1), for per-crater
      size, depth and presence;
    - the search is over the 3×3×3 neighbourhood of jittered feature points.

  fBm moves to 2a-ii, because its value-and-gradient form needs `snoise_grad`.

  Cellular noise follows Worley 1996 ("A Cellular Texture Basis Function",
  SIGGRAPH), implemented fresh. webgl-noise's `cellular3D.glsl` (MIT) returns
  only F1 and F2 with no feature vectors, so it cannot serve the gradients.
- **Rules that must die:**
  - `smin` value `≤ min(a,b)`, and it equals `min` (with that input's gradient)
    when `|a−b| ≥ k`.
  - `F1 ≤ F2`. F1 is the *nearest* feature, not the first found: use a fixture
    where the nearest lies in the last-visited cell.
  - **Invariant 8 property test:** every gradient matches central differences
    of its own `.w` within tolerance, at seeded random points, excluding a band
    around the F1 = F2 set (cellular borders).
  - The ids are constant within a feature's region.
- **Hands budget:** ~150k tokens.

#### 2a-ii. Simplex noise with analytic gradient, fBm, ridged noise, Phacelle

- **Adds:**
  - `snoise_grad(float3 p) → float4(∇n, n)`, ported from webgl-noise
    `src/noise3Dgrad.glsl` (Ashima Arts / McEwan, MIT, confirmed in
    `stegu/webgl-noise` today). It is the same noise family as the existing
    `snoise` (the same Ashima source, `CultMath.hlsl:82-84`), so the existing
    notice entry extends.
  - `fbm_grad(float3 p, int octaves, float lacunarity, float gain) → float4`:
    the octave sum of `snoise_grad`, with each octave's gradient scaled by its
    frequency.
  - `ridged_grad(float3 p, int octaves, float lacunarity, float gain) → float4`:
    `Σ aᵢ(1 − |nᵢ|)` with gradient `−Σ aᵢ fᵢ sign(nᵢ)∇nᵢ`. This is the operator's
    dune term ("1-abs(snoise(p)) gives nice dune contours", 2026-09-25). The
    crease at `nᵢ = 0` is real and deliberate. Ridged multifractal noise is
    standard (Musgrave, *Texturing and Modeling*, ch. 16) and is a noise
    primitive, not terrain shaping, so it belongs here.
  - `phacelle(float3 p, float3 side, float offset, float normalization) →
    CultPhasor { float4 cos; float4 sin; }`, each as `(∇.xyz, value.w)`:
    Johansen's Phacelle noise, generalised to 3D cells. The
    caller supplies `side`, the stripe's wave vector (upstream builds it as
    `perp(dir)·freq·τ`). In 3D the perpendicular is not unique, so choosing it
    belongs to the caller: on a sphere it is `cross(p̂, flow)`.
    - It visits 4×4×4 cells with jitter ±0.5 in each axis.
    - The weight is `max(0, exp(−2d²) − 0.01111)`, as upstream.
    - The output is normalised as upstream.
    - **The gradient is exact:** it includes the weight derivatives
      `∇w = −4v·exp(−2|v|²)` and the normalisation (`(I − ĉĉᵀ)∇P/|P|` above the
      threshold, `∇P/(1−normalization)` below it). Upstream's convention,
      `∇cos ≈ −sin·side`, ignores the weight gradient. The probe measured it
      against central differences: 16.8° off at the median and 97.8° at the 95th
      percentile. The exact form is 0.00° at the median and 0.03° at the 95th
      percentile. Invariant 8 therefore rules out upstream's shortcut here.
    - **Exact pruning:** skip any cell whose per-axis lower bound
      `Σ max(0, |pf − g| − 0.5)² ≥ 2.25`, because its weight is exactly 0 there
      (`exp(−4.5) = 0.011109 < 0.01111`). The pruned sum is bit-identical, and a
      test pins that. The probe measured 14.0 of 64 cells non-zero on average.
    - The provenance entry cites Rune Skovbo Johansen, Phacelle Noise, MPL-2.0
      (Shadertoy `t3dyWl`; erosion filter `wXcfWn`; blog, March 2026). CultMath's
      Unity package is MPL-2.0, so the licence is compatible. It also records
      that the 3D-cell generalisation and the caller-supplied side vector are
      new.
    - **No 2D variant.** Nothing consumes one. Add it when a consumer appears.
- **Phacelle is a primitive, not erosion code.** The evidence:
  - The function's parameters are point, direction, frequency, phase offset and
    normalisation. None of them is an erosion concept.
  - The upstream doc comment calls it "The Simple Phacelle Noise function
    produces a stripe pattern aligned with the input vector"
    (`lpmitchell/AdvancedTerrainErosion`, `AdvancedTerrainErosion.cs:423-449`,
    marked MPL-2.0, © 2025 Johansen).
  - The article calls it general-purpose, and it has its own standalone
    Shadertoy (`t3dyWl`).
  - The erosion-specific parts are the octave loop, masks, fade target and gully
    slope. They are 4b-ii's and stay in Asura.
- **Rules that must die:**
  - `snoise_grad.w` equals `snoise` within 1e-6 (report whether it is
    bit-equal).
  - fBm with 1 octave equals `snoise_grad`; gain and lacunarity act per octave.
  - **Invariant 8 property test:** for `snoise_grad`, `fbm_grad`, `ridged_grad`
    and both `phacelle` phasor components, the gradient matches central
    differences of `.w` at seeded random points. Exclusion bands:
    - `ridged_grad` excludes a band around `nᵢ = 0`, where the crease is real;
    - `phacelle` excludes a band around the normalisation threshold.

    A `ridged_grad` fixture straddling a zero crossing also pins that the two
    one-sided gradients differ in sign.
  - Phacelle:
    - output magnitude is 1 where the raw magnitude is ≥ 1 − normalization;
    - **stripes run along the flow**: with `side = cross(p̂, flow)` on the unit
      sphere, `mean|∂cos/∂flow| / mean|∂cos/∂side| ≤ 0.25`. The probe measured
      0.212, against 0.202 for the planar upstream function;
    - continuity: the max step difference shrinks tenfold per tenfold step;
    - pruned output is bit-equal to unpruned.
- **Verification (both parts, Yggdrasil):**
  - `dotnet test` from `packages/cultmath/tests/CultMath.Tests`.
    `HlslSourceCompatibilityTests.EveryMirrorFunctionMatchesCSharpMath`
    automatically compares every new mirror with its C# twin bit for bit; a
    mirror without a twin fails `Assert.Contains(... MirrorOnly)`.
  - Stryker `-t mtp --since:<part base>` (xunit.v3). Triage every survivor.
  - Starfire: FXC `cs_5_0` compile of a kernel calling each new function. This
    extends this map's FXC probe, and FXC is Unity's D3D11 compiler.
  - Also `dxc -T lib_6_3 -HV 2021` per `design.md`.
- **Hands budget:** ~200k tokens, including the release.
- **Ledger estimate (2a total):** +~250 C#, +~220 HLSL, +~350 test lines, and 2
  notice entries.

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

- **Repo/branch:** Asura `main` (re-pin at dispatch; `5f0f6bd` today). Hands
  commits code; Self commits the map.
- **Adds:**
  ```text
  unity/org.gamecult.asura/
    package.json                  name org.gamecult.asura, unity "6000.3"; no
                                  "dependencies": UPM package.json cannot declare
                                  git deps, so README states the required
                                  cultlib/cultmath tags
    README.md, CHANGELOG.md, LICENSE.md (MPL-2.0, Q9)
    Runtime/Core/GameCult.Asura.Core.asmdef      noEngineReferences: true;
                                  precompiledReferences GameCult.Geometry.dll, CultMath.dll
    Runtime/Core/*.cs             AsuraBodyDefinition { uint Seed; AsuraMaterial Material
                                  (Rock|Ice); bool Active; bool Eroded; bool Aeolian }
                                  (no radius: D3/Q5), and AsuraFieldKind { Planet, Sphere,
                                  Box, CraterRim, Plane, DuneCrest }. The analytic kinds
                                  are fixtures and a legit baseline, and they go through
                                  the same kernels as Planet (one rule, one path).
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
                                  ProjectReference to CultLib GameCult.Geometry and CultMath via a
                                  CultLibRoot sibling property (Aetheria.Shared's pattern,
                                  Aetheria.Shared.csproj:27-29), pinned to the cultlib-unity-v1.1.0 commit
    stryker-config.json           mutate **/unity/org.gamecult.asura/Runtime/Core/**/*.cs
  .gitignore                      Unity host dirs, bin/obj, StrykerOutput
  ```
- **Contract rules, headless (Core tests, Stryker):**
  - `AsuraBodyDefinition` equality and hash cover every field. Definitions that
    differ in any single field are unequal.
  - Seed-to-offset derivation (`AsuraSeed.Offsets(seed)`), one domain offset
    per term:
    - It runs in C# and is uploaded, so the HLSL never re-derives it.
    - It is deterministic.
    - Every component is bounded in `[0, 256)`, so noise keeps float precision.
    - Distinct seeds give distinct offsets across a 10k-seed sweep.
    - Distinct terms of one seed get distinct offsets, so craters and relief are
      not correlated.
    - It uses CultMath's `pcg3d`, not a local hash.
- **Verification:**
  - Yggdrasil: `dotnet test tests/Asura.Core.Tests` with a non-zero count, then
    Stryker.
  - Starfire: host batchmode EditMode run with one smoke test (the package
    compiles and Core types load). Log and PID in the scratchpad.
- **Hands budget:** ~150k tokens.

### Cut 4b. Asura: the planet field (4b-i, 4b-ii, 4b-iii)

The field is `f(p) = |p| − (1 + h(p̂))`, with `p̂ = p/|p|` (D3). It is a radial
height: `h` depends on direction only, so there are no overhangs, `f` is linear
along every ray from the centre, and every ray crosses `f = 0` exactly once, at
`r = 1 + h(p̂)`. The analytic fixture kinds (Box, CraterRim) are the only
non-radial fields. `h` composes terms chosen by the body's archetype.

**Archetype → terms** (target, Q4; exemplars from the target):

| Term | CultMath primitive (2a) | Used by |
| --- | --- | --- |
| macro relief | `fbm_grad` | every Planet body; amplitude per archetype |
| craters (bowl, rim, ejecta falloff; several scales) | `cellular_grad`, `smin` | rock-dead: dense, saturated (Moon, Mercury); ice-dead: dense, relaxed, i.e. shallower with softer rims (Ganymede, Callisto); every active body: sparse, small scales only |
| flood plains | smooth max of `h` with a flood level `L`, `−smin(−h, −L)` | rock-active (Io, Venus): lava-filled lows |
| lineae (ridges and grooves along cell borders; 2–3 scales) | `cellular_edge_grad` | ice-active (Europa, Enceladus) |
| aeolian dunes | `ridged_grad` (`1 − abs(snoise)`, stacked) | bodies with `Aeolian` (Mars, Titan, Venus exemplars). The lowest octave is **low frequency and high amplitude**: "push the terrain really far at really low frequency to influence the silhouette" (operator, 2026-09-25). A few giant crests bend the outline, with finer dune octaves on top. |
| hydraulic erosion | Johansen's filter, sphere-adapted (4b-iii), using `phacelle` | bodies with `Eroded` (Mars, Earth-like), layered over any archetype |

Asymmetric dunes (gentle windward, steep lee) are an optional tuning note, not a
requirement. Two ways to get them: a domain warp along a prevailing-wind
direction, or skewing `n` before `abs`. If either is tried, it is a parameter of
the dune term, not a new term.

**Parameters.** `Core/AsuraBodyParams.From(AsuraBodyDefinition)` produces the
struct the GPU reads: term enables, amplitudes, frequencies, scale counts,
densities, the flood level and `smin` k, dune octaves, erosion settings, and the
per-term offsets from 4a. Values are drawn from the seed inside per-archetype
ranges held in one table in Core. It also derives
**`A_max = Σ` the terms' amplitude bounds**, the most `h` can rise above 1. This
is parameter derivation, not field evaluation, so invariant 2 holds: C# never
computes `h`.

**Bounds and cell count** are a conscious trade, owned by Core:
- Half-extent `B = 1 + A_max + 1.5·c`, where `c` is the cell size.
- For a level of `n` samples per axis, `c = 2B/(n−1)`, solved together with `B`.
- **Level selection works from the projected size of `c`, not of the nominal
  radius.** A dune-heavy body with a large `A_max` spends more of its cells on
  empty shell, so for the same screen size it gets a larger `n` to keep the same
  cell size in pixels.
- The ratio `(1+A_max)` is logged per body so the benchmark can show the cost.

**D9 band limits** are computed in Core per dispatch. The footprint is uniform
within a dispatch: `c` for sampling, `c/N` for tiles. For each multi-octave term,
Core outputs the octave count and the last octave's fade weight, and the HLSL
reads them. So band selection is Stryker-reachable, and the kernels contain no
Nyquist logic of their own.

**Derivatives (invariant 8).** Every term returns value and analytic gradient
together, as `float4(∇.xyz, value.w)`, and composition uses the chain rule:
- crater profile: `P(F1)` with gradient `P′(F1)·∇F1`, with rims joined by
  `smin`, whose output gradient is the blended gradient;
- lineae: the profile of `F2 − F1` times its gradient;
- dunes: `ridged_grad`;
- flood plains: the smooth max `−smin(−h, −L)`, with `L`'s gradient 0;
- erosion: 4b-iii and Q12;
- the sphere composition `∇f = u − (I − uuᵀ)∇h/|p|`.

There are no finite differences anywhere in the field or its consumers.

**Minimum proving set: three exemplar bodies, covering every term kind:**
- **Moon:** rock, dead. Macro relief and dense craters.
- **Europa:** ice, active. Lineae and sparse craters.
- **Mars:** rock, dead, `Eroded` and `Aeolian`. Macro relief, craters, giant
  dune crests on the silhouette, and erosion.

Mars proves erosion over real crater walls and dune flanks, and it proves that
terms compose. The two remaining archetypes, rock-active (flood plains with
`smin`) and ice-dead (relaxed craters), are rows in the parameter table over
terms the set already exercises. They cost about a table row and a test row
each, so they land in 4b-ii with the lineae. If 4b-ii runs over budget, they are
the first items deferred.

This is three bodies rather than the brief's two-plus-erosion because the
aeolian ruling added a term kind, dunes, that neither craters nor lineae
exercise. Putting it on the eroded body instead of adding a fourth exemplar
keeps the set at three and tests term composition for free.

#### 4b-i. Field plumbing, grid sampling, readback, macro relief and craters (Moon)

- **Adds:**
  - `Runtime/Shaders/AsuraField.hlsl`, the **one** implementation of the field
    (invariant 2). It includes `Packages/org.gamecult.cultmath/Shaders/CultMath.hlsl`.
    - `float4 asura_height(float3 u, AsuraBodyGpu body, AsuraBand band)`
      returns `(∇h.xyz, h.w)`. This is the 3D gradient of the terms, before
      projection.
    - `float4 asura_field(float3 p, AsuraBodyGpu body, AsuraBand band)` returns
      `(∇f.xyz, f.w)`, built from `asura_height`, with
      `∇f = u − (I − uuᵀ)∇h/|p|`, where `u = p/|p|` (D8). At `|p| < ε` it returns a finite negative `f`,
      because the centre is inside.
    - The analytic kinds are implemented through the same entry point.
  - `Runtime/Shaders/AsuraSample.compute`: one kernel samples every pending
    body's grid into one flat `RWStructuredBuffer<float>`. A per-body table holds
    offset, dims, origin, `c`, params and band. The linear index within a body is
    `(x*ny + y)*nz + z`.
  - `Runtime/Shaders/AsuraEval.compute`: evaluates `asura_field` at a list of
    points and returns `(f, ∇f)`. Tests use it for continuity and gradient
    checks, and 7c's dock query uses it.
  - `Runtime/AsuraSampler.cs`:
    - Once per frame it packs pending bodies up to a sample budget, makes one
      dispatch and one `AsyncGPUReadback.Request(buffer, size, offset, cb)`.
    - It keeps a ring of K sample buffers. A buffer is never re-dispatched while
      its readback is in flight; when all K are in flight, it skips the frame.
    - In the callback, each body's slice is copied into a fresh `float[,,]` with
      one memcpy (`fixed (float* d = &grid[0,0,0])` and `UnsafeUtility.MemCpy`).
  - `Core/AsuraBodyParams` (the Moon rows), `Core/AsuraBounds` (B, c, level from
    projected `c`) and `Core/AsuraBand`.
- **Mechanism probed in pass 1:** a C# `float[,,]`'s memory is row-major.
  `Buffer.BlockCopy` of a flat `(x*ny+y)*nz+z` array into `float[2,3,4]` lands
  `[1,2,3] == 23`, so the GPU layout memcpys straight into the array `Extract`
  takes.
- **Authority map:**
  - Owner: `AsuraField.hlsl` owns shape. Core owns parameters, bounds and bands
    (numbers about the field, never values of it).
  - Inputs: definition, level and footprint.
  - Outputs: grid samples, and point evaluations.
  - Derived: the `float[,,]` copy is a cache, dropped once meshed.
  - Forbidden writers:
    - any C# evaluation of `h` or `f`;
    - anything writing the sample buffer except `AsuraSample.compute`;
    - any kernel-side octave or Nyquist logic.
- **Verification:**
  - Yggdrasil: Core tests and Stryker. The rules:
    - params are deterministic per seed and inside the archetype ranges;
    - `A_max ≥` the sum of enabled term bounds;
    - bounds satisfy `B ≥ 1 + A_max + c`;
    - level selection uses `c`: two bodies at the same screen size with
      different `A_max` get different `n`, and the larger `A_max` gets the larger
      `n`;
    - band octave count is monotone non-increasing in footprint and never
      exceeds the term's maximum;
    - the last octave's weight is in [0,1] and is 1 when the octave is far below
      Nyquist.
  - Starfire, host EditMode on the real GPU, one job:
    - assert `SystemInfo.supportsAsyncGPUReadback`; if it is false, Asura refuses
      loudly and keeps stand-ins;
    - **Layout:** the Plane kind (`f = a·p + b` with distinct a.x, a.y, a.z) on a
      5×7×9 grid reads back equal to the closed form at every index;
    - **Ring integrity:** 4K Plane bodies with distinct coefficients over
      consecutive PlayMode frames; every grid matches its own closed form;
    - **Radial exactness:** for Moon bodies, `AsuraEval` at `(1 + h(d))·d` gives
      `|f| ≤ ε` for 10k random directions;
    - **Closure:** for Moon × 64 seeds × 3 levels, every boundary sample is
      `> 0`;
    - **Gradient (invariant 8, the highest-risk site):** the analytic `∇f` of
      the full composed field, read back from `AsuraEval`, matches central
      differences of `f` (at h = 1e-3·c):
      - angle p99 < 0.5° and relative magnitude error p99 < 1e-2;
      - at seeded random points at **`|p|` ∈ {0.6, 1.0, 1.4}**, excluding a band
        of 2h around crater rims (cellular borders);
      - `|p| ≠ 1` is mandatory, because at `|p| = 1` a missing `1/|p|` factor is
        invisible. Moon terms produce a 3D gradient with a radial component, so
        a dropped `(I − uuᵀ)` projection fails too.
      - Also run the same check on the Sphere kind with a synthetic zonal term
        `h(u) = a·u.z`, whose `∇f` has a closed form. It pins the sign of the
        tangential term, in case both the implementation and FD are wrong the
        same way.
    - **Determinism:** the same definition sampled twice gives bit-equal grids
      (invariant 6, same machine);
    - **Seed:** two seeds give different grids.
  - negative: `rg -n "snoise|cellular|asura_field|asura_height" unity/org.gamecult.asura -g "*.cs"` → 0.
    C# never evaluates the field; band and params code must not call the
    primitives.
- **Hands budget:** ~200k tokens.

#### 4b-ii. The remaining terms: lineae, dunes, flood plains, relaxed craters (Europa, Mars minus erosion)

- **Adds:**
  - Lineae, from `cellular_edge_grad` at 2–3 scales, forming ridge pairs
    (Europa's double ridges) where `F2 − F1` is small.
  - Dunes, from `ridged_grad` with a low-frequency, high-amplitude first octave.
  - Flood plains, as the smooth max `−smin(−h, −L)`.
  - Relaxed crater profiles for ice.
  - The table rows for all four archetypes plus the `Aeolian` and `Eroded`
    switches (erosion's parameters exist, but its term is a no-op until 4b-iii).
  - The analytic `DuneCrest` kind, a single ridged crest line with a known
    closed form, used by 6c.
- **Verification (Starfire):**
  - closure for every archetype × `Aeolian` × 64 seeds × 3 levels;
  - the invariant-8 gradient check as in 4b-i for every archetype, masking a
    band around dune crests (`nᵢ = 0`) and lineae borders;
  - radial exactness per archetype;
  - **silhouette:** a Mars body without erosion, at a 12³ grid, shows its
    lowest dune octave in the mesh outline. Pin it as: the max radial extent
    over the mesh vertices exceeds `1 + 0.5·A_dune`;
  - Core:
    - lineae enabled iff the body is ice and active;
    - dunes enabled iff `Aeolian`;
    - dead bodies have a higher crater density than active ones at the same
      seed;
    - ice craters are shallower than rock craters.
- **Hands budget:** ~150k tokens.

#### 4b-iii. Hydraulic erosion on the sphere (Mars)

- **Adds:**
  - `Runtime/Shaders/AsuraErosion.hlsl`: Johansen's Advanced Terrain Erosion
    Filter, ported with attribution under MPL-2.0 (Q9 makes Asura MPL-2.0).
  - `unity/org.gamecult.asura/EROSION-PROVENANCE.md`, which records:
    - Rune Skovbo Johansen, Advanced Terrain Erosion Filter (© 2025, MPL-2.0),
      Shadertoy `wXcfWn`, explained at
      `blog.runevision.com/2026/03/fast-and-gorgeous-erosion-filter.html`;
    - the earlier work it credits, by Clay John (Shadertoy `MtGcWh`, MIT) and
      Felix Westin / Fewes (`7ljcRW`, MIT);
    - the reference transcription consulted: `github.com/lpmitchell/AdvancedTerrainErosion`,
      whose `ErosionFilter` and `PhacelleNoise` are marked MPL-2.0 and derived
      from `wXcfWn`;
    - that the sphere adaptation below is new. It is not in upstream, which is
      written for planar heightfields.

    Shadertoy returns 403 to automated fetches. If Hands needs the original
    shader text, the operator opens it in a browser. The lpmitchell port carries
    the procedure line for line.
  - **Not a source:** CultLib's July port (`gamecult_geometry_advanced_erosion_filter`
    and `spherical_erosion`, tag `b65619f`). Asura does not build on that line
    (target, "Not consumers"). Its `spherical_erosion` also lerps raw phases
    across cells instead of blending phasors, which is not Phacelle.
- **Derivatives (invariant 8; see Q12).** Erosion's gradient output is
  upstream's own analytic derivative. In each octave the slope contribution is
  `mask · gullyWeight · ∇cos · strength`, and the gully direction, masks and
  fade target are treated as locally constant. The probe shows this is **far
  from the exact derivative**: 15° median and 62° at the 95th percentile against
  central differences. That holds even with an exact Phacelle gradient: 13° and
  63°.
  - The gap is structural. The flow direction, the masks and the fade target
    all depend on `∇h_base`, so the exact first derivative of the eroded height
    needs `∇²h`, the second derivative, of every base term. The `sign()` in the
    gully update is nonsmooth as well.
  - So erosion cannot meet invariant 8's finite-difference tolerance without a
    Hessian flow through every term. **Q12** puts that fork to the operator.
    The map is written for recommendation A: upstream's derivative, with a
    measured, recorded bound, and no finite differences anywhere.
  - Curvature, the article's feature that needs a second derivative, is out of
    this campaign. Record it.
- **The sphere adaptation.** This is design; the probe below backs it.
  - Erosion runs in the **tangent space of the unit sphere**, entirely in 3D
    vectors. Its inputs:
    - the base `h`: every other term, band-limited;
    - the base tangential gradient `∇_T h = ∇h − p̂(p̂·∇h)`, analytic (D8);
    - the fade target, `clamp(h_base / (0.6·A_relief), −1, 1)`, as upstream.
  - The gully slope is a 3D tangent vector. It is re-projected onto the tangent
    plane after every octave's update (`g ← g − p̂(p̂·g)`).
  - Stripe cells live in **3D**: `phacelle(p̂·freq, side, 0.25, normalization)`,
    with `side = cross(p̂, normalize(gully)) · cellScale · τ`, which is tangent
    and perpendicular to the flow. The derivative direction is `side · −freq`,
    as upstream's `phacelle.zw · −freq`.
  - The output is a height delta added to `h`, and a slope delta added to
    `∇_T h` (D8).
  - Octave count and last-octave weight come from D9.
  - **Rejected: a cube-face domain** (a gnomonic uv per face, upstream 2D
    Phacelle, faces blended as in lpmitchell's `AccumulateFace` and
    `DirectionToFaceUv`). The probe below shows why.
- **Probe (this pass).** A scratch net10 C#
  prototype of the full octave loop, with a smooth analytic base height, scale
  0.15 and 5 octaves. It measured the max `|Δh|` between points a step `s` apart
  along 40 random great-circle arcs:

  | Variant | s = 1e-3 | 1e-4 | 1e-5 | Verdict |
  | --- | --- | --- | --- | --- |
  | 3D-cell Phacelle alone | 3.4e-1 | 3.5e-2 | 3.5e-3 | continuous (scales with s) |
  | erosion, 3D cells | 2.9e-3 | 3.1e-4 | 3.1e-5 | continuous across cell boundaries, no seams |
  | erosion, cube faces, hard switch | 2.5e-2 | 1.4e-2 | 1.2e-2 | **discontinuous**: a ~0.012 jump at face edges, about the size of the erosion itself |
  | erosion, cube faces, smoothstep blend (width 0.1 and 0.3) | 5.5e-3 | 6.5e-4 | 6.4e-5 | continuous, but erosion RMS near the seams is 0.73–0.75 of the interior: two unrelated gully networks are cross-faded, and gullies do not connect across the seam |

  More from the same probe:
  - Stripe alignment, `mean|∂/∂flow| / mean|∂/∂side|`: 0.212 on the sphere with
    3D cells, against 0.202 for upstream's planar 2D function. The adaptation
    keeps the stripes along the flow as well as upstream does.
  - Phacelle's own derivative: upstream's convention, `∇cos ≈ −sin·side`,
    ignores the weight gradient. Against central differences it is 16.8° off at
    the median and 97.8° at the 95th percentile. The **exact** gradient
    (weights and normalisation included) is 0.00° at the median and 0.03° at the
    95th percentile. So 2a-ii ships the exact form (invariant 8), and upstream's
    shortcut is not reproduced.
  - Cost, measured on CPU as a ratio only: 3D cells take 19.5 µs per 5-octave
    sample, against 5.8 µs for blended cube faces, 3.4× as much. On average only
    14.0 of the 64 visited cells have non-zero weight. 2a-ii's exact pruning and
    D9's band limits are the planned savings. GPU cost is measured in 5b.
- **Verification (Starfire, through `AsuraEval`):**
  - **Continuity on the GPU:** the probe's arc test (40 arcs; `s` = 1e-3, 1e-4,
    1e-5) on a Mars body. The max `|Δh|` must shrink at least 5× per 10× step.
  - **Switch:** with `Eroded` off, `h` is bit-equal to 4b-ii's output. With it
    on, `h` differs.
  - **Gradient accuracy, per Q12:**
    - Under A, report the angle distribution of the eroded `∇f` against central
      differences, and commit it as a regression bound. The probe's CPU
      baseline is p50 ≤ 20°, p95 ≤ 70°.
    - The **non-erosion part** of an eroded body's gradient (erosion's
      contribution subtracted) must still pass 4b-i's tight invariant-8 check.
      That proves the approximation is confined to the erosion term.
  - **Closure** for eroded bodies × 64 seeds: `A_max` must include erosion's
    magnitude, which upstream accumulates as `magnitude`.
  - operator: the Mars body in the host scene, orbiting camera. Gullies branch
    downhill off crater walls and dune flanks, and there is no visible seam or
    grid pattern.
- **Hands budget:** ~180k tokens.

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
  - `Core/AsuraLevels`: the projected size of the cell `c` (4b's bounds, which
    depend on `A_max`), not of the nominal radius, to grid level, from a
    quantised table (for example 8, 12, 16, 24, 32, 48, 64). Downgrades have
    hysteresis.
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
    - each proving-set body (Moon, Europa, Mars) at 3 levels meshes and closes
      on the **real sampled grid**, not a CPU fixture: every edge is used by an
      even number of quads, and exactly 2 at the tested seeds;
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
  - **Rows per archetype**, because erosion's per-sample cost dominates: Moon,
    Europa, Mars without erosion, and Mars with erosion. Each row records its
    `(1 + A_max)` shell factor.
  - A **field microbenchmark**: `AsuraEval` samples per millisecond for each row,
    so the grid-sampling cost is separable from readback and meshing.
  - Expectation per D8: one field evaluation per grid sample, not 4–7.
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
      step clamped to one base cell. Each iteration is **one** `asura_field`
      call, reading value and analytic gradient together (D8). There are no
      finite differences.
      - The field is a radial height, so it is linear along rays and has
        moderate slopes at low frequency. Newton is therefore well-conditioned
        everywhere except at crease discontinuities: dune crests, crater rims and
        lineae. There the gradient flips across the crease, and the step clamp
        plus 6c's snapping take over;
    - stores the analytic gradient normal and an archetype material, packed
      RGBA8. Rock and ice differ; so do crater floor, rim and ejecta, lineae,
      dune sand and eroded channels, using the terms' own masks. The gullies
      come from erosion's ridge map, which upstream emits beside the height.
    - evaluates with the tile footprint `c/N` (D9), so erosion and dune octaves
      that the grid could not resolve appear at tile resolution. The tile pass
      evaluates the same field, so no extra code is needed for this.

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
  - **Proving set:** on Moon, Europa and Mars (eroded), `|f(p)| ≤ ε·cell` for
    every point away from creases, and no point moves more than one base cell.
  - **Normals are the analytic gradient:** the stored normal equals
    `normalize(∇f)` from `AsuraEval` at the stored position, bit for bit. That
    proves the tile kernel reads the one gradient path and does not re-derive it.
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
  - **Watertight:** for Sphere, Moon, Europa and Mars (eroded) at N ∈ {1, 2, 4, 8} × 2 levels,
    read back `AsuraPatchExpand`. Each undirected triangle edge, keyed by the
    **exact bit patterns** of its two endpoints, appears an even number of times,
    and exactly twice on the Sphere. Report per case the edge count and the
    number of odd edges (must be 0).
  - **No folds:** each triangle's normal has a positive dot product with the
    field gradient at its centroid.
  - **Exclusivity:** the bare-mesh/patch switch never shows both and never
    neither, across a retile.
  - operator: Mars in the host scene, orbiting camera. The giant dune crests
    bend the silhouette, and erosion gullies appear as the camera closes in (N
    rises).
- **Hands budget:** ~180k tokens.

### Cut 6c. Asura: crease snapping from field gradients in the tile pass

- **Adds**, in the tile kernels, after refinement, for every point (vertex points
  included, so N = 1 gets sharp base vertices and the mesher stays pure surface
  nets):
  - It evaluates `asura_field` on a stencil of fixed offsets in
    **body-local space** and reads the analytic gradients (D8; no finite
    differences). The offsets depend only on (body, grid level): never on the
    quad, N or patch coordinates.
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
  - **DuneCrest** kind (4b-ii: a ridged `1 − |n|` crest with a closed form), at
    N ∈ {1, 2, 4, 8}:
    - patch points within one cell of the crest line land within ε of it;
    - zero folds; watertight;
    - the fixture includes a **low-frequency, high-amplitude crest that shows in
      the silhouette at a 12³ grid**.
  - **Small-body silhouette (N = 1 is load-bearing).** The operator's giant dune
    crests are creases *on the outline*. At 10³–12³, surface nets rounds a crest
    to about the grid spacing, and a rock rendered at N = 1 has no patch
    interior to recover it. So snapping the base vertices is the only thing that
    keeps the crest sharp on small bodies.
    - Test: a Mars body at 12³ with N = 1. The max radial extent of the snapped
      base vertices near the crest comes within ε·c of the analytic crest height.
      Without snapping it falls short by about a cell, so the test also reports
      the unsnapped shortfall as its control.
  - **Eroded control:** on eroded Mars, report the snapped-point count and prove
    zero folds. Under Q12's recommendation, erosion's gradient is upstream's
    approximation. False crease families in gully networks would show here as a
    count out of proportion to the real creases (crests, rims). Report it for
    Self; do not tune it away.
  - The benchmark gets one more run to record snapping's cost.
- **Hands budget:** ~180k tokens.

### Cut R. Asura release tag

- `asura-unity-v0.1.0` on the commit that closes 6c. Package version `0.1.0` and
  CHANGELOG. The README states the required cultlib and cultmath tags. Soul
  passes before the tag is pushed.

### Cut 7a. Aetheria: planet definitions from zone generation (headless)

- **Repo/branch:** Aetheria, `hands/asura-7a`, from `master` after fire-control
  merges (Q1). Worktree. Re-pin anchors at dispatch.
- **Per-file changes (master `9b85211f`):**
  - `Assets/Scripts/ServerShared/ZoneData.cs:69-73`: `PlanetData` gains:
    - `[Key(9)] uint Seed`;
    - `[Key(10)] BodyMaterial Material`, a ServerShared enum `Rock | Ice`;
    - `[Key(11)] bool Active`;
    - `[Key(12)] bool Eroded`;
    - `[Key(13)] bool Aeolian`.

    Aetheria owns the definition (target identity table), so the vocabulary is
    Aetheria's. 7b maps it field by field onto `AsuraBodyDefinition` at the
    ZoneRenderer seam, and ServerShared never references Asura (negative check
    below). The document version follows Aetheria's schema-evolution rule for
    `aetheria.planetdata`; Hands reads that rule before choosing.
  - **Old records (Q3, ruled 2026-09-25):** a `PlanetData` with `Seed == 0`
    fails loudly on load, and the run regenerates. There is no compatibility
    path. The loud failure needs its own test.
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
    - The archetype comes from the seed (Q3), drawn from the same **local**
      generator after the seed. The material and activity draw is uniform over
      the four archetypes. `Eroded` and `Aeolian` are independent draws, at the
      probabilities in Q10. The probabilities live as `ZoneGenerationSettings`
      entries, so a later content pass can tune them, or band them by mass,
      without code.
- **Contract rules (`tests/Aetheria.Shared.Tests`, Stryker via the fire-control
  line's `stryker-config.json`):**
  - The same galaxy zone gives the same seeds and archetypes.
  - Across a 10k-zone sweep, archetype frequencies match the configured
    probabilities within binomial tolerance. This kills a hard-coded archetype
    and a draw that ignores its probability.
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
    call `Asura.Add(key, AsuraBodyDefinition{Seed, Material, Active, Eroded,
    Aeolian}, planet.Body.transform, planet.Body)`. The mapping from
    `PlanetData` is the one seam between the two vocabularies, and it has a test
    covering every field.
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
  - `EngineAssetCheck.Run` exits 0;
  - operator: enter zones and see rocky and icy bodies (cratered, lined, duned,
    eroded) varying by seed and stable across reloads. Gas giants and suns are
    unchanged.
- **Hands budget:** ~180k tokens.

### Cut 7c. Docked view: dock-site query and raised N with per-edge stitching

- **Q6, ruled 2026-09-25: presentation only.**
  - The station stands on its parent rocky planet, at a direction derived from
    the station's key.
  - Simulation positions are unchanged.
  - Stations whose parent is not a rocky `PlanetData` keep today's dock view.
  - The flattened pad waits.
- **Adds (Asura):**
  - The dock query is `AsuraEval` (4b-i) plus a closed form. The field is a
    radial height (4b), so the surface along direction `u` is exactly
    `(1 + h(u))·u`. It is one `asura_height` evaluation, and its analytic
    gradient gives the normal `normalize(∇f)`. There is no marching and no
    bisection. The query uses a fixed small footprint (D9), so the station sits
    on full detail. Readback is async. This is the target's field query; it
    never reads the atlas.
  - `SetFocus(key, direction, radius)`: quads whose centres lie within the focus
    radius get `N_hi`, and the rest get the body's N.
  - Per-edge factor, keyed by the mesh edge (D4): `E(edge) = max(N_a, N_b)`.
  - The edge region holds `E − 1` points per edge. `asura_patch_vertex` stitches
    each quad's boundary rows to its edges' factors.
  - The layout moves from uniform slots to per-quad offsets, computed in Core.
- **Adds (Aetheria), per Q6:**
  - The dock-site direction is derived from the station's record key, a stable
    hash mapped to a unit vector. It lives in ServerShared or Unity-side
    Aetheria code, never in Asura, because Aetheria owns it (target identity
    table). Test: the same key gives the same direction, and distinct keys give
    distinct directions.
  - `ActionGameManager.cs:863-881` (`DoDock`): when the parent orbit body is a
    rocky `PlanetData`, frame the close-up. The station asset is placed at the
    query point and aligned to the normal, and `SetFocus` is called. Otherwise
    the current dock camera path applies unchanged. `Undock` (`:883+`) clears
    the focus.
- **Verification:**
  - Starfire:
    - on the Sphere kind, the query returns `dir ± ε` (unit radius), with
      normal = dir. On Moon, `AsuraEval` at the returned point gives
      `|f| ≤ ε`;
    - watertight: 6b's bit-edge check at **mixed** N (focus on, `N_hi`
      ∈ {4, 8, 16} beside N ∈ {1, 2}), zero odd edges and zero folds;
    - Core Stryker on the per-edge factor and offsets.
  - operator: dock at a station orbiting a rocky planet. The station sits on the
    surface without floating or sinking, and there is no crack at the N
    boundary.
- **Hands budget:** ~200k tokens. The flattened pad (a `smin_grad` blend of a
  plane into `h`) is deferred by Q6. If it is ever ruled in, it is its own cut.

## Subtraction ledger (estimates)

| Cut | Removed | Added | Deps/targets |
| --- | --- | --- | --- |
| 1 | 310 files, 11,344 text lines, 16 LFS binaries, ~60 C#/YAML lines, 2 prefabs + 1 asset (Q2) | 0 | −1 third-party plugin |
| 2a-i/ii | 0 | ~300 C#, ~260 HLSL, ~400 test, notices entries | cultmath-unity 0.3.0 |
| 2 | 0 | ~220 C#, ~300 test, 1 md, 1 Stryker config | none |
| 3 | 0 | ~20 script, 1 asmdef line, 2 metas, 1 DLL+pdb | cultlib-unity 1.1.0 |
| 4a | 0 | package skeleton, host project, ~150 C#, 1 test csproj | +1 Unity package, +1 host project, +1 test project |
| 4b-i/ii/iii | 0 | ~450 HLSL, ~350 C#, ~450 test, 1 provenance md | none |
| 5a/5b | 0 | ~350 C#, ~250 test, 1 results doc | none |
| 6a/6b/6c | 0 | ~450 HLSL, ~250 C#, ~400 test, 2 results docs | none |
| 7a/7b/7c | ~5 lines | ~150 Aetheria C#, ~250 Asura, ~200 test | Aetheria +1 UPM dep (asura) |

This is net additive, and deliberately so: the campaign buys a capability that
did not exist (Asura). Cut 1 pays about 11k lines of unused third-party plugin
toward it.

## Operator questions

### Answered (2026-09-25; the target's "Rulings on the cut map's questions" owns the wording)

- **Q1 (Aetheria line):** Cuts 1 and 7 land on `master` after fire-control
  merges. This was the recommendation. Applied in Cuts 1, 7a and 7b.
- **Q2 (outpost and recon assets):** delete them with the plugin. Applied in
  Cut 1.
- **Q3 (seeds, archetype, old saves):**
  - Seed from a local generator that never draws from the zone stream.
  - Archetype by seed for now.
  - Old saves with no seed fail loudly and regenerate, with no compatibility
    path.

  Applied in Cut 7a.
- **Q4 (the second biome):** superseded by a larger ruling. Bodies are
  archetypes: material (rock | ice) × activity (dead | active), with hydraulic
  erosion switched on per body, using Johansen's erosion filter. The operator
  added aeolian dunes, also switched on per body, from `1 − |snoise|`, with a
  low-frequency, high-amplitude first octave "to influence the silhouette". And
  analytic derivatives at every level (invariant 8). Applied in 2a, 4a, 4b,
  5b, 6a, 6c and 7a.
- **Q5:** the field is unit-radius and the body transform scales it (D3).
- **Q6:** the docked view is presentation only. The station stands on its
  parent planet at a direction derived from its key. The pad waits (7c).
- **Q7:** invariant 6 is bitwise on one machine and within tolerance across
  machines.
- **Q8:** no asteroid belts in this campaign.
- **Q9:** Asura is MPL-2.0.

### Open

- **Q10. How often do the per-body switches fire?** Zone gen draws `Eroded`
  and `Aeolian` from the planet's seed (7a). What probabilities should it use,
  and may they apply to any archetype?
  - A: independent draws, `Eroded` at 0.25 and `Aeolian` at 0.25, over any
    archetype, as the target says ("layered over any of these"). Both are
    `ZoneGenerationSettings` entries.
  - B: only rock bodies may be eroded or aeolian.
  - C: tie both to an atmosphere notion that Aetheria does not have yet.

  **Recommended: A.** It is the ruling's literal reading, and it is tunable as
  data. C would invent a domain concept, atmospheres, that nobody has asked
  for. This blocks 7a's draw, not Asura.
- **Q11. Band-limiting (D9): Self's target amendment, not an operator fork.**
  Invariant 1 says the field is the only truth. Under D9, each lowering
  evaluates the same field band-limited to its footprint, so the grid does not
  alias gullies and crests. Proposed wording: "The field is the only truth; a
  lowering may band-limit it to what its sample spacing can resolve, and never
  otherwise alters it." Self decides whether that needs the operator.
- **Q12. Erosion and invariant 8.** The exact first derivative of the eroded
  height depends on `∇²h` of every base term:
  - the gully flow direction, the masks and the fade target all depend on
    `∇h_base`;
  - the `sign()` in the gully update is nonsmooth.

  Upstream's own analytic derivative treats those as locally constant. Against
  central differences on the sphere (probe, 4b-iii) it is 15° off at the median
  and 62° at the 95th percentile. It stays at 13° and 63° with an exact Phacelle
  gradient, so Phacelle is not the cause.
  - A: accept upstream's analytic derivative for the erosion term only.
    - It still involves no finite differences.
    - Its measured bound is committed as a regression guard.
    - The non-erosion part of the gradient must still pass the tight check.
    - Shading on eroded bodies then uses upstream's gradients, which is how the
      article's own renders are shaded.
  - B: carry second derivatives (Hessians) through every CultMath primitive
    and term, so erosion's derivative is exact by the chain rule.
    - This roughly doubles 2a.
    - It still leaves the `sign()` nonsmooth.
    - It is also what the article's curvature feature would need.
  - C: change the erosion so its masks and flow do not depend on slope. That
    changes the look upstream was chosen for.

  **Recommended: A,** with curvature and second derivatives recorded as out of
  this campaign. Invariant 8's purpose, analytic gradients with no finite
  differences, holds. What A gives up is only that the erosion term's gradient
  matches the finite difference of its height. This blocks nothing until 4b-iii;
  2a is sized for A.

## Findings for Self (not assigned to a cut)

Pass 1's findings that the target has absorbed (verified at `5f0f6bd`):
- the plugin's wiring (in "Not consumers");
- the July planetary line (in "Not consumers");
- the mesh-edge identity row;
- Q7's wording of invariant 6.

Still open from pass 1:
- **The target's substrate paragraph** still says "flat body/icon/gravity-well
  quads". `Body` is `high-res-sphere.fbx` with `Planet.mat`; only
  `Minimap Icon` and `Gravity Well` are quads
  (`Planet.prefab:15,43,98,126,254,282`). It still pins cultlib 1.0.60 and
  cultmath 0.2.4, which are the fire-control line's. Master pins 1.0.59 and
  0.2.3.
- **Aetheria's `settings-globals-cut.md` fork B** (`:527-535`) is superseded by
  F2. Sweep it the day Cut 1 lands.
- **`ZoneRenderer.cs:497`** spins planets per frame, not per second. This is an
  Aetheria follow-up.

New in pass 2 (checked against the target at `5f0f6bd`):
- **Target invariant 3** still names only `cultmath_snoise(float3)`. The field
  now rests on the whole 2a set: `snoise_grad`, `fbm_grad`, `ridged_grad`,
  `cellular`, `smin_grad` and `phacelle`.
- **Target invariant 7** still says "the existing flat planet icon stands in".
  The stand-in is the `Body` sphere (7b).
- **Target invariant 8** says "every term is checked against central finite
  differences away from its known creases". Erosion cannot pass that without
  second derivatives (probe). Q12 must be ruled before 4b-iii, and the
  invariant's wording must follow the ruling.
- **Target invariant 2 wording is too narrow now.** It says "the CPU never
  evaluates the planet field". Core does evaluate numbers *about* the field:
  parameters, `A_max`, bounds and band limits. The map's forbidden-writer line
  draws the boundary: "never values of it". The target may want that sentence.
- **Shadertoy is unreadable to agents.** `shadertoy.com` returns 403 to
  automated fetches, so Johansen's original shader text (`wXcfWn`, `t3dyWl`)
  was not read in this pass. The procedure was read from
  `lpmitchell/AdvancedTerrainErosion`, whose Phacelle and erosion functions are
  marked MPL-2.0 and derived from `wXcfWn`, and from the article. If exactness
  against the original matters, the operator can paste it. This is recorded as
  missing substrate.
- **Unity sphere implementations exist upstream of us, and neither fits.**
  lpmitchell's cube-face blend is the approach the probe rejects (seams, or
  cross-faded gullies). CultLib's July `spherical_erosion` lerps raw phases
  across cells, which is not Phacelle. Neither is a source.
- **voidbot MCP** was unreachable in pass 1. Pass 2 did not retry it; it worked
  from git, `gh` and probes.
