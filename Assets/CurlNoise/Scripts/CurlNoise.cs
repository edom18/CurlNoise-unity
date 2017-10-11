using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CurlNoiseSample
{
    public class CurlNoise : MonoBehaviour
    {
        [SerializeField]
        private ComputeShader _shader;

        private void Start()
        {
            int kernelIndex = _shader.FindKernel("CurlNoiseMain");

            int num = 10;
            ComputeBuffer buffer = new ComputeBuffer(num, sizeof(int));

            _shader.SetBuffer(kernelIndex, "Result", buffer);
            _shader.Dispatch(kernelIndex, num, 1, 1);

            int[] data = new int[num];
            buffer.GetData(data);

            for (int i = 0; i < data.Length; i++)
            {
                Debug.Log(data[i]);
            }

            buffer.Release();
        }
    }
}
