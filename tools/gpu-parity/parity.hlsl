// Parity kernel. CultMath.hlsl is included, not copied: fxc is run with /I pointing at the pinned
// CultLib checkout. The only code here is buffer plumbing plus the branch probe below.
#include "CultMath.hlsl"

StructuredBuffer<float4> points : register(t0);          // xyz = 3D point, xy = 2D point
RWStructuredBuffer<float4> grad_out : register(u0);      // cultmath_snoise_grad: xyz = gradient, w = value
RWStructuredBuffer<float4> value_out : register(u1);     // x = snoise(float3), y = snoise(float2), z/w = branch codes

[numthreads(64, 1, 1)]
void main(uint3 id : SV_DispatchThreadID)
{
    uint n, stride;
    points.GetDimensions(n, stride);
    if (id.x >= n) return;
    float3 p = points[id.x].xyz;

    grad_out[id.x] = cultmath_snoise_grad(p);

    // Branch probe: re-derives the simplex-ordering comparisons exactly as cultmath_snoise(float3) and
    // cultmath_snoise(float2) write them, to read back which side of each float-decided tie was taken.
    // It is a separate expression tree, so it witnesses the comparison, not the compiled noise body.
    float3 x0 = p - floor(p + dot(p, (1.0 / 3.0).xxx)) + dot(floor(p + dot(p, (1.0 / 3.0).xxx)), (1.0 / 6.0).xxx);
    uint code3 = (step(x0.y, x0.x) > 0.5 ? 1u : 0u) | (step(x0.z, x0.y) > 0.5 ? 2u : 0u) | (step(x0.z, x0.x) > 0.5 ? 4u : 0u);
    float2 q = p.xy;
    float2 i2 = floor(q + dot(q, (0.366025403784439).xx));
    float2 y0 = q - i2 + dot(i2, (0.211324865405187).xx);
    uint code2 = y0.x > y0.y ? 1u : 0u;

    value_out[id.x] = float4(cultmath_snoise(p), cultmath_snoise(q), asfloat(code3), asfloat(code2));
}
