using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class DateTimeExtension
{
    static private System.DateTime UNIX_EPOCH = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);

    static public long GetUnixTime(this System.DateTime date)
    {
        System.DateTime utc = date.ToUniversalTime();
        System.TimeSpan elapsedTime = utc - UNIX_EPOCH;
        return (long)elapsedTime.TotalSeconds;
    }
}

public class PerlinNoise
{
    private Xorshift _xorshift;
    private int[] _p;

	public PerlinNoise(uint seed)
    {
        _xorshift = new Xorshift(seed);

        int[] p = new int[256];
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = (int)Mathf.Floor(_xorshift.Random() * 256);
        }

        int[] p2 = new int[512];
        for (int i = 0; i < p2.Length; i++)
        {
            p2[i] = p[i & 255];
        }

        _p = p2;
    }

    private float Noise(float x, float y = 0, float z = 0)
    {
        int X = (int)Mathf.Floor(x) & 255;
        int Y = (int)Mathf.Floor(y) & 255;
        int Z = (int)Mathf.Floor(z) & 255;

        x -= Mathf.Floor(x);
        y -= Mathf.Floor(y);
        z -= Mathf.Floor(z);

        float u = Fade(x);
        float v = Fade(y);
        float w = Fade(z);

        int[] p = _p;

        int A, AA, AB, B, BA, BB;

        A = p[X + 0] + Y; AA = p[A] + Z; AB = p[A + 1] + Z;
        B = p[X + 1] + Y; BA = p[B] + Z; BB = p[B + 1] + Z;

        return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA + 0], x + 0, y + 0, z + 0),
                                       Grad(p[BA + 0], x - 1, y + 0, z + 0)),
                               Lerp(u, Grad(p[AB + 0], x + 0, y - 1, z + 0),
                                       Grad(p[BB + 0], x - 1, y - 1, z + 0))),
                       Lerp(v, Lerp(u, Grad(p[AA + 1], x + 0, y + 0, z - 1),
                                       Grad(p[BA + 1], x - 1, y + 0, z - 1)),
                               Lerp(u, Grad(p[AB + 1], x + 0, y - 1, z - 1),
                                       Grad(p[BB + 1], x - 1, y - 1, z - 1))));
	}

    public float OctaveNoise(float x, int octaves)
    {
        float result = 0;
        float amp = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            result += Noise(x) * amp;
            x *= 2.0f;
            amp *= 0.5f;
        }

		return result;
    }

    public float OctaveNoise(float x, float y, int octaves)
    {
        float result = 0;
        float amp = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            result += Noise(x, y) * amp;
            x *= 2.0f;
            y *= 2.0f;
            amp *= 0.5f;
        }

		return result;
    }

     public float OctaveNoise(float x, float y, float z, int octaves)
     {
        float result = 0;
        float amp = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            result += Noise(x, y, z) * amp;
            x *= 2.0f;
            y *= 2.0f;
            z *= 2.0f;
            amp *= 0.5f;
        }

		return result;
     }

    private float Fade(float t)
    {
        return t * t * t * (t * (t * 6f - 15f) + 10f);
    }

    private float Lerp(float t, float a, float b)
    {
        return a + t * (b - a);
    }

    private float Grad(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = (h < 8) ? x : y;
        float v = (h < 4) ? y : (h == 12 || h == 14) ? x : z;
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
