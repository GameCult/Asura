using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using CultMath;
using Vortice.Direct3D;
using Vortice.Direct3D11;
using Vortice.DXGI;

// GPU parity for cultmath_snoise(float2/float3) and cultmath_snoise_grad vs C# math.snoise / snoise_grad.
// See README.md. Exit code 0 only if the pin holds and every flag's errors are inside Tolerance.
internal static class Tolerance
{
    // Absolute error bounds per input-magnitude tier (|p| <= 8, <= 128, above): 1.35x the maximum measured on
    // a GTX 1070 for both fxc flag sets (README.md, "Measured results"). The error scales with ulp(|p|).
    public static readonly float[] ValueAbs = { 4.2e-6f, 7.2e-5f, 1.65e-3f };
    public static readonly float[] GradAbs = { 3.3e-5f, 4.2e-4f, 6.8e-3f };
    public const float RelFloor = 0.01f; // relative error = |gpu - ref| / max(|ref|, RelFloor)
}

internal static class Program
{
    const string PinTag = "cultmath-unity-v0.5.0";

    static int Main(string[] args)
    {
        string cultlib = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .First(a => a.Key == "CultLibRoot").Value!;
        string here = AppContext.BaseDirectory;
        string kernel = FindUp(here, "parity.hlsl");
        string shaderDir = Path.Combine(cultlib, "packages", "cultmath", "shaders");

        string head = Git(cultlib, "rev-parse HEAD");
        string pin = Git(cultlib, $"rev-parse {PinTag}^{{commit}}");
        Console.WriteLine($"CultLib {cultlib}\n  HEAD {head}\n  {PinTag} {pin}");
        if (head != pin) { Console.Error.WriteLine("PIN FAILED: CultLib HEAD is not the pinned tag."); return 3; }

        var pts = BuildPoints(out int tieCount);
        Console.WriteLine($"points: {pts.Length / 4} ({tieCount} tie-constructed, {pts.Length / 4 - tieCount} seeded random)");
        var cs = Reference(pts);

        string fxc = Environment.GetEnvironmentVariable("FXC")
            ?? @"C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\fxc.exe";

        using var device = CreateDevice(out string gpu);
        Console.WriteLine($"GPU: {gpu}");

        bool ok = true;
        foreach (var (name, flags) in new[] { ("default", ""), ("/Gis", "/Gis") })
        {
            string cso = Path.Combine(Path.GetTempPath(), $"snoise-parity-{name.Trim('/')}.cso");
            Fxc(fxc, flags, shaderDir, kernel, cso);
            var gpuRes = Dispatch(device, File.ReadAllBytes(cso), pts);
            ok &= Report(name, pts, tieCount, cs, gpuRes);
        }
        Console.WriteLine(ok ? "RESULT: within tolerance" : "RESULT: OUT OF TOLERANCE");
        return ok ? 0 : 1;
    }

    // ---- points ----
    static float[] BuildPoints(out int tieCount)
    {
        var list = new List<float>();
        void Add(float x, float y, float z) { list.Add(x); list.Add(y); list.Add(z); list.Add(0f); }
        // Tie boundaries: coordinates that are dyadic (multiples of 1/64) so that value.x - value.y = k
        // is exact, which makes x0.x == x0.y an exact float tie in every evaluation order. Each pattern
        // also gets 1 and 2 ulp neighbours of one coordinate, where the tie is decided by rounding.
        var a = Enumerable.Range(-16, 33).Select(i => i / 8f).Concat(new[] { 0.1015625f, 3.390625f, -7.703125f, 100.25f, 1000.140625f });
        var b = new[] { -3.5f, -0.25f, 0f, 0.3125f, 1.75f, 5.0f, 37.5f };
        foreach (float av in a) foreach (float bv in b) foreach (int k in new[] { -2, -1, 0, 1, 2 })
        {
            float t = av + k;
            for (int u = -2; u <= 2; u++)
            {
                float p = Ulps(av, u), q = Ulps(t, u);
                Add(t, p, bv); Add(bv, t, p); Add(t, bv, p);   // x==y, y==z, x==z (offset by k)
                Add(q, av, bv); Add(bv, q, av); Add(q, bv, av);
            }
            Add(t, av, av); Add(av, t, av); Add(av, av, t);
        }
        foreach (float av in a) Add(av, av, av);
        tieCount = list.Count / 4;
        var rng = new System.Random(0x5A5E);
        foreach (float scale in new[] { 4f, 64f, 1024f })
            for (int i = 0; i < 40000; i++)
                Add((float)(rng.NextDouble() * 2 - 1) * scale, (float)(rng.NextDouble() * 2 - 1) * scale, (float)(rng.NextDouble() * 2 - 1) * scale);
        return list.ToArray();
    }

    static float Ulps(float v, int n)
    {
        for (; n > 0; n--) v = MathF.BitIncrement(v);
        for (; n < 0; n++) v = MathF.BitDecrement(v);
        return v;
    }

    // ---- C# reference ----
    record struct Result(float[] Grad4, float[] Value3, float[] Value2, uint[] Code3, uint[] Code2);

    static Result Reference(float[] pts)
    {
        int n = pts.Length / 4;
        var r = new Result(new float[n * 4], new float[n], new float[n], new uint[n], new uint[n]);
        for (int i = 0; i < n; i++)
        {
            var p = new float3(pts[4 * i], pts[4 * i + 1], pts[4 * i + 2]);
            var g = math.snoise_grad(p);
            r.Grad4[4 * i] = g.x; r.Grad4[4 * i + 1] = g.y; r.Grad4[4 * i + 2] = g.z; r.Grad4[4 * i + 3] = g.w;
            r.Value3[i] = math.snoise(p);
            r.Value2[i] = math.snoise(new float2(p.x, p.y));
            // same expressions as parity.hlsl's probe, through CultMath's own operators
            var i0 = math.floor(p + math.dot(p, new float3(1f / 3f, 1f / 3f, 1f / 3f)));
            var x0 = p - i0 + math.dot(i0, new float3(1f / 6f, 1f / 6f, 1f / 6f));
            r.Code3[i] = (math.step(x0.y, x0.x) > 0.5f ? 1u : 0u) | (math.step(x0.z, x0.y) > 0.5f ? 2u : 0u) | (math.step(x0.z, x0.x) > 0.5f ? 4u : 0u);
            var q = new float2(p.x, p.y);
            var j = math.floor(q + math.dot(q, new float2(0.366025403784439f, 0.366025403784439f)));
            var y0 = q - j + math.dot(j, new float2(0.211324865405187f, 0.211324865405187f));
            r.Code2[i] = y0.x > y0.y ? 1u : 0u;
        }
        return r;
    }

    // ---- D3D11 ----
    static ID3D11Device CreateDevice(out string name)
    {
        var res = D3D11.D3D11CreateDevice(null, DriverType.Hardware, DeviceCreationFlags.None,
            new[] { FeatureLevel.Level_11_0 }, out ID3D11Device? dev);
        if (res.Failure || dev is null) throw new Exception($"D3D11CreateDevice failed: {res}");
        using var dxgi = dev.QueryInterface<IDXGIDevice>();
        using var adapter = dxgi.GetAdapter();
        name = adapter.Description.Description;
        return dev;
    }

    static Result Dispatch(ID3D11Device dev, byte[] cso, float[] pts)
    {
        int n = pts.Length / 4;
        var ctx = dev.ImmediateContext;
        using var cs = dev.CreateComputeShader(cso);

        using var inBuf = dev.CreateBuffer(pts, new BufferDescription((uint)(n * 16), BindFlags.ShaderResource, ResourceUsage.Default,
            CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 16));
        using var inSrv = dev.CreateShaderResourceView(inBuf, new ShaderResourceViewDescription
        {
            Format = Format.Unknown,
            ViewDimension = ShaderResourceViewDimension.Buffer,
            Buffer = new BufferShaderResourceView { FirstElement = 0, NumElements = (uint)n },
        });

        (ID3D11Buffer buf, ID3D11UnorderedAccessView uav, ID3D11Buffer staging) Out()
        {
            var b = dev.CreateBuffer(new BufferDescription((uint)(n * 16), BindFlags.UnorderedAccess, ResourceUsage.Default,
                CpuAccessFlags.None, ResourceOptionFlags.BufferStructured, 16));
            var u = dev.CreateUnorderedAccessView(b, new UnorderedAccessViewDescription
            {
                Format = Format.Unknown,
                ViewDimension = UnorderedAccessViewDimension.Buffer,
                Buffer = new BufferUnorderedAccessView { FirstElement = 0, NumElements = (uint)n },
            });
            var s = dev.CreateBuffer(new BufferDescription((uint)(n * 16), BindFlags.None, ResourceUsage.Staging,
                CpuAccessFlags.Read, ResourceOptionFlags.None, 0));
            return (b, u, s);
        }
        var (gBuf, gUav, gStage) = Out();
        var (vBuf, vUav, vStage) = Out();

        ctx.CSSetShader(cs);
        ctx.CSSetShaderResource(0, inSrv);
        ctx.CSSetUnorderedAccessView(0, gUav);
        ctx.CSSetUnorderedAccessView(1, vUav);
        ctx.Dispatch((uint)((n + 63) / 64), 1, 1);
        ctx.CopyResource(gStage, gBuf);
        ctx.CopyResource(vStage, vBuf);
        ctx.Flush();

        float[] Read(ID3D11Buffer stage)
        {
            var o = new float[n * 4];
            var m = ctx.Map(stage, 0, MapMode.Read, Vortice.Direct3D11.MapFlags.None);
            Marshal.Copy(m.DataPointer, o, 0, o.Length);
            ctx.Unmap(stage, 0);
            return o;
        }
        var g = Read(gStage);
        var v = Read(vStage);
        var r = new Result(g, new float[n], new float[n], new uint[n], new uint[n]);
        for (int i = 0; i < n; i++)
        {
            r.Value3[i] = v[4 * i]; r.Value2[i] = v[4 * i + 1];
            r.Code3[i] = (uint)v[4 * i + 2]; r.Code2[i] = (uint)v[4 * i + 3];
        }
        foreach (var d in new IDisposable[] { gBuf, gUav, gStage, vBuf, vUav, vStage }) d.Dispose();
        return r;
    }

    // ---- compare ----
    sealed class Stat
    {
        public double MaxAbs, MaxRel; public int Nan; public long Over; readonly List<float> errs = new();
        public void Add(float gpu, float refv, float tolAbs)
        {
            if (float.IsNaN(gpu) || float.IsInfinity(gpu)) { Nan++; return; }
            double d = Math.Abs((double)gpu - refv);
            errs.Add((float)d);
            MaxAbs = Math.Max(MaxAbs, d);
            MaxRel = Math.Max(MaxRel, d / Math.Max(Math.Abs(refv), Tolerance.RelFloor));
            if (d > tolAbs) Over++;
        }
        string Pct()
        {
            if (errs.Count == 0) return "";
            errs.Sort();
            float P(double q) => errs[(int)Math.Min(errs.Count - 1, q * errs.Count)];
            return $"  p50 {P(.5):E1} p99 {P(.99):E1} p99.9 {P(.999):E1} p99.99 {P(.9999):E1}";
        }
        public override string ToString() => $"maxAbs {MaxAbs:E3}  maxRel {MaxRel:E3}  NaN/Inf {Nan}  over-tol {Over}{Pct()}";
    }

    // Tiers by max |coordinate|: x0 = value - i + ... cancels, so the input's own ulp scales the error.
    static readonly (string Name, float Max)[] Tiers = { ("|p|<=8", 8f), ("8<|p|<=128", 128f), ("128<|p|", float.MaxValue) };

    static bool Report(string flag, float[] pts, int tieCount, Result cs, Result gpu)
    {
        int n = pts.Length / 4;
        Console.WriteLine($"\n== fxc flags: {flag} ({n} points) ==");
        bool ok = true;
        foreach (var (set, lo, hi) in new[] { ("ties", 0, tieCount), ("random", tieCount, n) })
        for (int t = 0; t < Tiers.Length; t++)
        {
            var idx = Enumerable.Range(lo, hi - lo).Where(i =>
            {
                float m = Math.Max(Math.Abs(pts[4 * i]), Math.Max(Math.Abs(pts[4 * i + 1]), Math.Abs(pts[4 * i + 2])));
                return m <= Tiers[t].Max && (t == 0 || m > Tiers[t - 1].Max);
            }).ToArray();
            if (idx.Length == 0) continue;
            var s3 = new Stat(); var s2 = new Stat(); var gv = new Stat(); var gg = new Stat();
            int m3 = 0, m2 = 0;
            float va = Tolerance.ValueAbs[t], ga = Tolerance.GradAbs[t];
            foreach (int i in idx)
            {
                s3.Add(gpu.Value3[i], cs.Value3[i], va);
                s2.Add(gpu.Value2[i], cs.Value2[i], va);
                for (int c = 0; c < 3; c++) gg.Add(gpu.Grad4[4 * i + c], cs.Grad4[4 * i + c], ga);
                gv.Add(gpu.Grad4[4 * i + 3], cs.Grad4[4 * i + 3], va);
                if (gpu.Code3[i] != cs.Code3[i]) m3++;
                if (gpu.Code2[i] != cs.Code2[i]) m2++;
            }
            Console.WriteLine($"-- {set} {Tiers[t].Name}: {idx.Length} points, tol value {va:E1} grad {ga:E1}\n   snoise(float3)       {s3}\n   snoise(float2)       {s2}\n   snoise_grad .w value  {gv}\n   snoise_grad .xyz      {gg}\n   branch mismatches (informational): float3 {m3}, float2 {m2}");
            ok &= s3.Over + s2.Over + gv.Over + gg.Over == 0 && s3.Nan + s2.Nan + gv.Nan + gg.Nan == 0;
        }
        return ok;
    }

    // ---- plumbing ----
    static void Fxc(string fxc, string flags, string includeDir, string kernel, string cso)
    {
        var psi = new ProcessStartInfo(fxc) { RedirectStandardOutput = true, RedirectStandardError = true };
        foreach (var a in new[] { "/nologo", "/T", "cs_5_0", "/E", "main", "/I", includeDir, "/Fo", cso }) psi.ArgumentList.Add(a);
        if (flags != "") psi.ArgumentList.Add(flags);
        psi.ArgumentList.Add(kernel);
        using var p = Process.Start(psi)!;
        string o = p.StandardOutput.ReadToEnd() + p.StandardError.ReadToEnd();
        p.WaitForExit();
        if (p.ExitCode != 0) throw new Exception($"fxc {flags} failed:\n{o}");
        Console.WriteLine($"fxc {(flags == "" ? "(default)" : flags)} ok -> {cso}");
    }

    static string Git(string dir, string args)
    {
        var psi = new ProcessStartInfo("git", $"-C \"{dir}\" {args}") { RedirectStandardOutput = true };
        using var p = Process.Start(psi)!;
        string o = p.StandardOutput.ReadToEnd().Trim();
        p.WaitForExit();
        return o;
    }

    static string FindUp(string dir, string file)
    {
        for (var d = new DirectoryInfo(dir); d != null; d = d.Parent)
            if (File.Exists(Path.Combine(d.FullName, file))) return Path.Combine(d.FullName, file);
        throw new FileNotFoundException(file);
    }
}
