using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Assertions;

namespace CurlNoiseSample
{
    public struct Particle
    {
        public int id;
        public bool active;
        public Vector3 position;
        public Vector3 velocity;
        public Vector3 color;
        public float scale;
        public float time;
        public float liefTime;
    }

    public class CurlParticle : MonoBehaviour
    {
        // 1メッシュに入る最大長点数
        const int MAX_VERTEX_NUM = 65534;

        #region ### パーティクル設定 ###
        [Header("==== パーティクル設定 ====")]
        [SerializeField]
        private int _maxParticleNum = 1000;

        [SerializeField]
        private float _minLifeTime = 1f;

        [SerializeField]
        private float _maxLifeTime = 5f;

        [SerializeField]
        private Mesh _mesh;

        [SerializeField]
        private Shader _shader;
        #endregion ### パーティクル設定 ###

        #region ### カールノイズ設定 ###
        [Header("==== カールノイズ設定 ====")]
        [SerializeField]
        ComputeShader _computeShader;

        [SerializeField]
        private float _speedFactor = 1.0f;

        [SerializeField]
        private Transform _sphere;

        [SerializeField]
        private Color _particleColor;

        [SerializeField]
		private float[] _noiseScales = new [] { 100f, 10f, 5f, };

		[SerializeField]
		private float[] _noiseGain = new [] { 1.0f, 0.5f, 0.25f, };

        [SerializeField]
        private int _seed = 100;

        [SerializeField]
        private int _octaves = 5;

        [SerializeField]
        private float _frequency = 5.0f;

        [SerializeField]
        [Range(0f, 1f)]
        private float _curlNoiseIntencity = 1f;
        #endregion ### カールノイズ設定 ###

        #region ### Private fields ###
        private int[] _p;
        private ComputeBuffer _buff;
        private Mesh _combinedMesh;
        private List<Material> _materials = new List<Material>();

        private ComputeBuffer _particles;

        private Xorshift _xorshift;

        private int _kernelIndex;
        private int _particleNumPerMesh;
        private int _meshNum;
        #endregion ### Private fields ###

        #region ### MonoBehaviour ###
        private void OnDisable()
        {
            _particles.Release();
            _buff.Release();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdatePosition();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position, Vector3.one * _noiseScales[0]);

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, Vector3.one * _noiseScales[1]);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(transform.position, Vector3.one * _noiseScales[2]);
        }
        #endregion ### MonoBehaviour ###

        /// <summary>
        /// 指定した数分、マージしたメッシュを生成する
        /// </summary>
        /// <param name="mesh">元となるメッシュ</param>
        /// <param name="num">生成するメッシュ数</param>
        /// <returns>マージされたメッシュ</returns>
        private Mesh CreateCombinedMesh(Mesh mesh, int num)
        {
            Assert.IsTrue(mesh.vertexCount * num <= MAX_VERTEX_NUM);

            int[] meshIndices = mesh.GetIndices(0);
            int indexNum = meshIndices.Length;

            // Buffer
            int[] indices = new int[num * indexNum];
            List<Vector2> uv0 = new List<Vector2>();
            List<Vector2> uv1 = new List<Vector2>();
            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Vector4> tangents = new List<Vector4>();

            for (int id = 0; id < num; id++)
            {
                vertices.AddRange(mesh.vertices);
                normals.AddRange(mesh.normals);
                tangents.AddRange(mesh.tangents);
                uv0.AddRange(mesh.uv);

                // 各メッシュのIndexは、1つのモデルの頂点数 * ID分ずらす
                for (int n = 0; n < indexNum; n++)
                {
                    indices[id * indexNum + n] = id * mesh.vertexCount + meshIndices[n];
                }

                // 2番目のUVにIDを格納しておく
                for (int n = 0; n < mesh.uv.Length; n++)
                {
                    uv1.Add(new Vector2(id, id));
                }
            }

            Mesh combinedMesh = new Mesh();
            combinedMesh.SetVertices(vertices);
            combinedMesh.SetIndices(indices, MeshTopology.Triangles, 0);
            combinedMesh.SetNormals(normals);
            combinedMesh.RecalculateNormals();
            combinedMesh.SetTangents(tangents);
            combinedMesh.SetUVs(0, uv0);
            combinedMesh.SetUVs(1, uv1);
            combinedMesh.RecalculateBounds();
            combinedMesh.bounds.SetMinMax(Vector3.one * -100f, Vector3.one * 100f);

            return combinedMesh;

        }

        /// <summary>
        /// Compute Shaderを使って位置を更新する
        /// </summary>
        private void UpdatePosition()
        {
            float frequency = Mathf.Clamp(_frequency, 0.1f, 64.0f);
            int octaves = Mathf.Clamp(_octaves, 1, 16);

            if (_buff == null)
            {
                _buff = new ComputeBuffer(512, sizeof(int));
                _buff.SetData(_p);
            }

            _computeShader.SetInt("_Octaves", octaves);
            _computeShader.SetFloat("_Frequency", frequency);
            _computeShader.SetBuffer(_kernelIndex, "_P", _buff);

            Vector3 p = _sphere.transform.position;
            _computeShader.SetFloats("_NoiseScales", _noiseScales);
			_computeShader.SetFloats("_NoiseGain", _noiseGain);
			_computeShader.SetFloat("_PlumeBase", -_noiseScales[0] * 0.5f);
			_computeShader.SetFloat("_PlumeHeight", _noiseScales[0]);
            _computeShader.SetFloat("_CurlNoiseIntencity", _curlNoiseIntencity);
            _computeShader.SetFloats("_SphereCenter", new[] { p.x, p.y, p.z });
            _computeShader.SetFloat("_SphereRadius", _sphere.transform.lossyScale.x * 0.5f);
            _computeShader.SetFloat("_SpeedFactor", _speedFactor);
            _computeShader.SetBuffer(_kernelIndex, "_Particles", _particles);
            _computeShader.SetFloat("_DeltaTime", Time.deltaTime);

            _computeShader.Dispatch(_kernelIndex, _maxParticleNum / 8, 1, 1);

            for (int i = 0; i < _meshNum; i++)
            {
                Material material = _materials[i];
                material.SetInt("_IdOffset", _particleNumPerMesh * i);
                material.SetBuffer("_Particles", _particles);
                Graphics.DrawMesh(_combinedMesh, transform.position, transform.rotation, material, 0);
            }
        }

        /// <summary>
        /// パーリンノイズ用のグリッドを生成する
        /// </summary>
        /// <returns></returns>
        private int[] CreateGrid()
        {
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

            return p2;
        }

        /// <summary>
        /// パーティクルを生成する
        /// </summary>
        /// <returns></returns>
        private Particle[] GenerateParticles()
        {
            Particle[] particles = new Particle[_maxParticleNum];

            for (int i = 0; i < _maxParticleNum; i++)
            {
                float x = Random.Range(-1f, 1f);
                float y = Random.Range(-3f, -1f);
                float z = Random.Range(-1f, 1f);

                float r = _particleColor.r;
                float g = _particleColor.g;
                float b = _particleColor.b;

                Particle p = new Particle
                {
                    id = i,
                    active = true,
                    position = new Vector3(x, y, z),
                    color = new Vector3(r, g, b),
                    scale = 1.0f,
                    time = 0,
                    liefTime = Random.Range(_minLifeTime, _maxLifeTime),
                };

                particles[i] = p;
            }

            return particles;
        }

        /// <summary>
        /// 初期化処理
        /// </summary>
        private void Initialize()
        {
            int seed = Mathf.Clamp(_seed, 0, 2 << 30 - 1);
            _xorshift = new Xorshift((uint)seed);
            _p = CreateGrid();

            _particleNumPerMesh = MAX_VERTEX_NUM / _mesh.vertexCount;
            _meshNum = (int)Mathf.Ceil((float)_maxParticleNum / _particleNumPerMesh);

            for (int i = 0; i < _meshNum; i++)
            {
                Material material = new Material(_shader);
                material.SetInt("_IdOffset", _particleNumPerMesh * i);
                _materials.Add(material);
            }

            _combinedMesh = CreateCombinedMesh(_mesh, _particleNumPerMesh);

            _particles = new ComputeBuffer(_maxParticleNum, Marshal.SizeOf(typeof(Particle)));

            Particle[] particles = GenerateParticles();
            _particles.SetData(particles);

            _kernelIndex = _computeShader.FindKernel("CurlNoiseMain");
        }
    }
}
