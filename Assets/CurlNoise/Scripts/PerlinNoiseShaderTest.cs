using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseShaderTest : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _shader;

    [SerializeField]
    private GameObject _target;

    [SerializeField]
    private int _width = 128;

    [SerializeField]
    private int _height = 128;

    [SerializeField]
    private int _seed = 100;

    [SerializeField]
    private int _octaves = 5;

    [SerializeField]
    private float _frequency = 5.0f;

    private Material _material;
    private RenderTexture _texture;

    private void Start()
    {
        _material = _target.GetComponent<Renderer>().material;
        _texture = new RenderTexture(_width, _height, 1);
        _texture.enableRandomWrite = true;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Perform();
        }
    }

    private int[] CreateGrid(uint seed)
    {
        Xorshift xorshift = new Xorshift((uint)_seed);

        int[] p = new int[256];
        for (int i = 0; i < p.Length; i++)
        {
            p[i] = (int)Mathf.Floor(xorshift.Random() * 256);
        }

        int[] p2 = new int[512];
        for (int i = 0; i < p2.Length; i++)
        {
            p2[i] = p[i & 255];
        }

        return p2;
    }

    private void Perform()
    {
        float frequency = Mathf.Clamp(_frequency, 0.1f, 64.0f);
        int octaves = Mathf.Clamp(_octaves, 1, 16);
        int seed = Mathf.Clamp(_seed, 0, 2 << 30 - 1);

        int[] p = CreateGrid((uint)seed);

        float fx = (float)_width / frequency;
        float fy = (float)_height / frequency;

        ComputeBuffer buff = new ComputeBuffer(512, sizeof(int));
        buff.SetData(p);

        RenderTexture texture = new RenderTexture(_width, _height, 1);
        texture.enableRandomWrite = true;

        int kernelID = _shader.FindKernel("PerlinNoiseMain");
        _shader.SetInt("_Octaves", octaves);
        _shader.SetFloat("_Fx", fx);
        _shader.SetFloat("_Fy", fy);
        _shader.SetBuffer(kernelID, "_P", buff);
        _shader.SetTexture(kernelID, "Result", texture);

        _shader.Dispatch(kernelID, _width / 8, _height / 8, 1);

        _material.mainTexture = texture;

        buff.Release();
    }
}
