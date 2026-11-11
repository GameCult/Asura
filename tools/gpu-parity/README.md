# GPU parity: CultMath simplex noise on D3D11

Checks that `cultmath_snoise(float2)`, `cultmath_snoise(float3)` and `cultmath_snoise_grad`
(`float4`: gradient in `.xyz`, value in `.w`) from CultLib's `packages/cultmath/shaders/CultMath.hlsl`,
compiled by FXC and run as a D3D11 compute shader, match C# `math.snoise` / `math.snoise_grad`
(`packages/cultmath`) within a stated tolerance. The shader text under test is CultLib's own file:
`parity.hlsl` `#include`s it through `fxc /I`. Nothing is copied.

Windows only (D3D11 hardware device, `fxc.exe`). Needs a CultLib checkout at tag `cultmath-unity-v0.5.0`;
the run fails its pin check if CultLib `HEAD` is not that tag's commit.

```powershell
git clone https://eureka.gamecult.org/forge/GameCult/CultLib.git
git -C CultLib checkout cultmath-unity-v0.5.0
$env:CULTLIB_ROOT = "$PWD\CultLib"          # default: a sibling directory named CultLib
# optional: $env:FXC = "...\fxc.exe"        # default: Windows Kits 10.0.26100.0 x64
dotnet run -c Release --project tools/gpu-parity/GpuParity.csproj
```

It compiles the kernel twice (default flags, where FMA contraction is allowed, and `/Gis` IEEE strict),
dispatches once per flag (about 130k threads), and prints max abs / max relative error per function,
split into tie-constructed points and a seeded random sweep. Exit code 0 means within `Tolerance`
(in `Program.cs`) and no branch mismatches.

Points: dyadic coordinates on the simplex tie planes (`x0.x == x0.y`, `y == z`, `x == z`, offsets
k in -2..2) with 0, 1 and 2 ulp neighbours, plus 3 x 40000 seeded uniform points at scales 4, 64, 1024.
`parity.hlsl` also reads back the ordering comparisons (a re-derivation of the `step`/`>` tests, so it
witnesses the comparison, not the compiled noise body); a mismatch against C# is reported.
Relative error is `|gpu - ref| / max(|ref|, 0.01)`.

The GPU is shared: the run is one short dispatch per flag.

## Measured results

See the PR description for the latest run.
