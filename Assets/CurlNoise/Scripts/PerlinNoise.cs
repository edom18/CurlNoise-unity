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

    private void Noise(float x, float y = 0, float z = 0)
    {

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
