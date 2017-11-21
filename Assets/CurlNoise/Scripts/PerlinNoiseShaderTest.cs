using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseShaderTest : MonoBehaviour
{
    [SerializeField]
    private ComputeShader _shader;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            Perform();
        }
    }

    private void Perform()
    {
        int width = 128;
        int height = 128;
        int num = width * height;
        ComputeBuffer buff = new ComputeBuffer(num, sizeof(float));

        int kernelID = _shader.FindKernel("PerlinNoiseMain");
        _shader.SetBuffer(kernelID, "_P", buff);
    }
}
