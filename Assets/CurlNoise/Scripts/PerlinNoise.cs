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

public class Xorshift
{
    private uint[] _vec = new uint[4];

    public Xorshift(uint seed = /* +new Date */ 10)
    {
        for (uint i = 1; i <= 4; i++)
        {
            seed = 1812433253 * (seed ^ (seed >> 30)) + i;
            _vec[i - 1] = seed;
        }
    }

    public float Random()
    {
        uint t = _vec[0];
        uint w = _vec[3];

        _vec[0] = _vec[1];
        _vec[1] = _vec[2];
        _vec[2] = w;

        t ^= t << 11;
        t ^= t >> 8;
        w ^= w >> 19;
        w ^= t;

        _vec[3] = w;

        return w * 2.3283064365386963e-10f;
    }
}

public class PerlinNoise : MonoBehaviour
{
    private Xorshift _xorshift;
    private int[] _p;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_xorshift == null)
            {
                _xorshift = new Xorshift((uint)System.DateTime.Now.GetUnixTime());
            }
            Debug.Log(_xorshift.Random());
        }
    }

    private void Start()
    {
        _xorshift = new Xorshift((uint)System.DateTime.Now.GetUnixTime());

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
        int X = (int)Mathf.Floor(x)) & 255;
        int Y = (int)Mathf.Floor(y)) & 255;
        int Z = (int)Mathf.Floor(z)) & 255;

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

        return return Lerp(w, Lerp(v, Lerp(u, Grad(p[AA + 0], x + 0, y + 0, z + 0),
                                              Grad(p[BA + 0], x - 1, y + 0, z + 0)),
                                      Lerp(u, Grad(p[AB + 0], x + 0, y - 1, z + 0),
                                              Grad(p[BB + 0], x - 1, y - 1, z + 0))),
                              Lerp(v, Lerp(u, Grad(p[AA + 1], x + 0, y + 0, z - 1),
                                              Grad(p[BA + 1], x - 1, y + 0, z - 1)),
                                      Lerp(u, Grad(p[AB + 1], x + 0, y - 1, z - 1),
                                              Grad(p[BB + 1], x - 1, y - 1, z - 1))))
    }

    private float OctaveNoise1(float x, int octaves)
    {
        float result = 0;
        float amp = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            result += Noise(x) * amp;
            x *= 2.0f;
            amp *= 0.5f;
        }

        return result 
    }

    private float OctaveNoise2(float x, float y, int octaves)
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

        return result
    }

     private float OctaveNoise3: (x, y, z, octaves)
     {
        float float result = 0;
        float amp = 1.0f;

        for (int i = 0; i < octaves; i++)
        {
            result += Noise(x, y, z) * amp;
            x *= 2.0f;
            y *= 2.0f;
            z *= 2.0f;
            amp *= 0.5f;
        }

        return result
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
