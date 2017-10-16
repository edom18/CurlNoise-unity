﻿using System.Collections;
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

        [SerializeField]
        ComputeShader _computeShader;

        [SerializeField]
        private float _speedFactor = 1.0f;

        [SerializeField]
        private Transform _sphere;

        [SerializeField]
        private Color _particleColor;

        private Mesh _combinedMesh;
        private List<Material> _materials = new List<Material>();

        private ComputeBuffer _particles;

        private int _kernelIndex;
        private int _particleNumPerMesh;
        private int _meshNum;

        private void OnDisable()
        {
            _particles.Release();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdatePosition();
        }

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

        private void UpdatePosition()
        {
            Vector3 p = _sphere.transform.position;
            _computeShader.SetFloats("_SphereCenter", new[] { p.x, p.y, p.z });
            _computeShader.SetFloat("_SphereRadius", _sphere.transform.lossyScale.x);
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

        private Particle[] GenerateParticles()
        {
            Particle[] particles = new Particle[_maxParticleNum];

            for (int i = 0; i < _maxParticleNum; i++)
            {
                float x = Random.Range(-50f, 50f);
                float y = Random.Range(-50f, 50f);
                float z = Random.Range(-50f, 50f);

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

        private void Initialize()
        {
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

            // ランダムな値を初期値として与える
            _computeShader.SetFloat("_RandomX1", Random.value);
            _computeShader.SetFloat("_RandomY1", Random.value);
            _computeShader.SetFloat("_RandomZ1", Random.value);
            _computeShader.SetFloat("_RandomX2", Random.value);
            _computeShader.SetFloat("_RandomY2", Random.value);
            _computeShader.SetFloat("_RandomZ2", Random.value);
        }
    }
}
