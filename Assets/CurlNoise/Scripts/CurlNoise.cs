using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;

namespace CurlNoiseSample
{
    public class CurlNoise : MonoBehaviour
    {
        [SerializeField]
        private Transform[] _targets;

        [SerializeField]
        private ComputeShader _shader;

        private ComputeBuffer _results;
        private ComputeBuffer _positions;
        private int _kernelIndex;

        private void OnDisable()
        {
            _results.Release();
            _positions.Release();
        }

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            int num = _targets.Length;

            Vector3[] positions = _targets.Select(t => t.position).ToArray();
            _positions.SetData(positions);

            _shader.SetBuffer(_kernelIndex, "Result", _results);
            _shader.SetBuffer(_kernelIndex, "Positions", _positions);
            _shader.Dispatch(_kernelIndex, num / 8 + 1, 1, 1);

            Vector3[] data = new Vector3[num];
            _results.GetData(data);

            for (int i = 0; i < _targets.Length; i++)
            {
                _targets[i].position += data[i];
            }
        }

        private void Initialize()
        {
            _kernelIndex = _shader.FindKernel("CurlNoiseMain");

            int num = _targets.Length;

            _results = new ComputeBuffer(num, Marshal.SizeOf(typeof(Vector3)));
            _positions = new ComputeBuffer(num, Marshal.SizeOf(typeof(Vector3)));


            // ランダムな値を初期値として与える
            _shader.SetFloat("randomX1", Random.value);
            _shader.SetFloat("randomY1", Random.value);
            _shader.SetFloat("randomZ1", Random.value);
            _shader.SetFloat("randomX2", Random.value);
            _shader.SetFloat("randomY2", Random.value);
            _shader.SetFloat("randomZ2", Random.value);
        }
    }
}
