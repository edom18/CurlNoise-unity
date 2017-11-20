using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseTest : MonoBehaviour
{
	private PerlinNoise _perlinNoise;

	[SerializeField]
	private float _frequency = 5.0f;

	[SerializeField]
	private int _octaves = 5;

	[SerializeField]
	private int _seed = 100;

	[SerializeField]
	private int _width = 128;

	[SerializeField]
	private int _height = 128;

	[SerializeField]
	private GameObject _quad;

	private Material _material;

	private Texture2D _texture;

	private void Start()
	{
		_material = _quad.GetComponent<Renderer>().material;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.T))
		{
			DoNoise();
		}
	}

//	###
//	frequency [0.1 .. 8.0 .. 64.0]
//	octaves   [1 .. 8 .. 16]
//	seed      [0 .. 2^30 - 1]
//	###
	private void DoNoise()
	{
		float frequency = Mathf.Clamp(_frequency, 0.1f, 64.0f);
		int octaves = Mathf.Clamp(_octaves, 1, 16);
		int seed = Mathf.Clamp(_seed, 0, 2 << 30 - 1);

		PerlinNoise perlinNoise = new PerlinNoise((uint)seed);

		float fx = (float)_width / frequency;
		float fy = (float)_height / frequency;

		if (_texture == null)
		{
			_texture = new Texture2D(_width, _height);
		}

		Color[] pixels = new Color[_width * _height];
		for (int i = 0; i < pixels.Length; i++)
		{
			int x = i % _width;
			int y = i / _width;
			float n = perlinNoise.OctaveNoise(x / fx, y / fy, octaves);
			float c = Mathf.Clamp(218f * (0.5f + n * 0.5f), 0f, 255f) / 255f;
			pixels[i] = new Color(c, c, c, 1f);
		}

		_texture.SetPixels(pixels);
		_texture.Apply();

		_material.mainTexture = _texture;
	}
}
