
float3 Mod289(float3 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 Mod289(float4 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 Permute(float4 x)
{
    return Mod289(((x * 34.0) + 1.0) * x);
}

float4 TaylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float SnoiseScalar(float3 v)
{
    const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    const float4 D = float4(0.0, 0.5, 1.0, 2.0);

    // First Corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

    // Other Corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    // x0 = x0 - 0.0 + 0.0 * C.xxx;
    // x1 = x0 -  i1 + 1.0 * C.xxx;
    // x2 = x0 -  i2 + 2.0 * C.xxx;
    // x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy; //  2.0 * C.x = 1 / 3 = C.y
    float3 x3 = x0 - D.yyy;      // -1.0 + 3.0 * C.x = -0.5 = -D.y

    // Permutations
    i = Mod289(i);

    float4 p = Permute(Permute(Permute(
          i.z + float4(0.0, i1.z, i2.z, 1.0))
        + i.y + float4(0.0, i1.y, i2.y, 1.0))
        + i.x + float4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49 * 6 = 294)
    float n_ = 0.142857142857; // 1.0 / 7.0
    float3 ns = n_ * D.wyz - D.xzx;

    float4 j = p - 49.0 * floor(p * ns.z * ns.z); // mod(p, 7*7)

    float4 x_ = floor(j * ns.z);
    float4 y_ = floor(j - 7.0 * x_); // mod(j, N);

    float4 x = x_ * ns.x + ns.yyyy;
    float4 y = y_ * ns.x + ns.yyyy;
    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    // float4 s0 = float4(lessThan(b0, 0.0)) * 2.0 - 1.0;
    // float4 s1 = float4(lessThan(b1, 0.0)) * 2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, float4(0.0, 0.0, 0.0, 0.0));

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 p0 = float3(a0.xy, h.x);
    float3 p1 = float3(a0.zw, h.y);
    float3 p2 = float3(a1.xy, h.z);
    float3 p3 = float3(a1.zw, h.w);

    // Normalise gradients
    float4 norm = TaylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));

    p0 *= norm.x;
    p1 *= norm.y;
    p3 *= norm.z;
    p0 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;

    return 42.0 * dot(m * m, float4(
        dot(p0, x0),
        dot(p1, x2),
        dot(p2, x2),
        dot(p3, x3)
    ));
}


// シンプレクスノイズ関数
// 3Dとして3要素を計算。
// それぞれのノイズは明らかに違う（極端に大きなオフセット）を持たせた値とする
// Reference: https://www.cs.ubc.ca/~rbridson/docs/bridson-siggraph2007-curlnoise.pdf
float3 Snoise(float3 vec)
{
    float x = SnoiseScalar(vec);

    float y = SnoiseScalar(float3(
        vec.y + 31.416,
        vec.z - 47.853,
        vec.x + 12.793
    ));

    float z = SnoiseScalar(float3(
        vec.z - 233.145,
        vec.x - 113.408,
        vec.y - 185.31
    ));

    return float3(x, y, z);
}
