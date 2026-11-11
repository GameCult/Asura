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

Run 2026-11-11 on Starfire, NVIDIA GeForce GTX 1070, D3D11 feature level 11_0, fxc 10.0.26100.0
(`cs_5_0`), CultLib `7a8933b0` (`cultmath-unity-v0.5.0`). 163928 points: 43928 tie-constructed, 120000
seeded random (3 x 40000 at scales 4, 64, 1024). Default and `/Gis` produced the same maxima to three
digits; no NaN or Inf.

Maximum absolute error, both flag sets, by max |coordinate| of the input:

| tier | snoise(float3) | snoise(float2) | grad `.w` | grad `.xyz` |
| --- | --- | --- | --- | --- |
| <= 8 | 3.1e-6 | 3.1e-6 | 3.1e-6 | 2.4e-5 |
| 8 to 128 | 5.3e-5 | 4.2e-5 | 5.3e-5 | 3.1e-4 |
| > 128 (to 1024) | 1.2e-3 | 4.9e-4 | 1.2e-3 | 5.0e-3 |

Max relative error (floor 0.01): 4.8e-2 for values and 2.1e-1 for gradients, both at |p| > 128 near zero
crossings. Median absolute error is 0 to 2e-7 in every tier.

Stated tolerance (`Tolerance` in `Program.cs`): absolute error <= 4.2e-6, 7.2e-5, 1.65e-3 for values and
<= 3.3e-5, 4.2e-4, 6.8e-3 for gradient components, per tier above (1.35x the measured maximum). Basis:
the error scales with ulp of the input magnitude (x0 = value - i + ... cancels, and GPU and CPU round
the first steps differently), not with the noise: 16x more input magnitude gave 17x more error between
the first two tiers. Single GPU and driver; a different driver may need the bounds re-measured.

Tie handling: under both flag sets the GPU took a different simplex ordering branch than C# at
190 of 35814 tie points (float3) and 39 (float2) with |p| <= 8, 2 and 4 in the middle tier, 0 above
(the same counts under `/Gis`, so it is not FMA contraction; the probe is a separate expression tree,
so it is evidence of a GPU/CPU rounding difference in x0, not of the noise body's own path). Every one of
those points is inside the value and gradient tolerance: the noise is continuous across the tie, so a
flipped branch costs at most the ulp-scale error above. Branch mismatches are reported, not failed.
