using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurPostPass : ScriptableRenderPass
{
    public int iterations = 3;
    public float blurSpread = 0.6f;
    public int downSample = 2;
    static readonly string renderTag = "GaussianBlurFeature";
    public Material material = null;
    public Shader shader = null;

    RenderTargetIdentifier cameraTexture;
    RenderTargetIdentifier tmpRT;

    int tmpRTId = Shader.PropertyToID("_GaussianBlurTexture");

    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cmd.GetTemporaryRT(tmpRTId, Screen.width, Screen.height, 0, FilterMode.Bilinear, RenderTextureFormat.ARGB32);
        tmpRT = new RenderTargetIdentifier(tmpRTId);
        ConfigureTarget(tmpRT);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        cameraTexture = renderingData.cameraData.renderer.cameraColorTarget;
        CommandBuffer cmd = CommandBufferPool.Get(renderTag);
        // cmd.Blit(cameraTexture, tmpRT, material);
        // cmd.Blit(tmpRT, cameraTexture);
        Render(cmd, ref renderingData);
        // 执行命令缓冲区
        context.ExecuteCommandBuffer(cmd);
        cmd.Clear();
        // 释放命令缓存
        CommandBufferPool.Release(cmd);
    }


    private void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // 获取目标相机的描述信息
        RenderTextureDescriptor rtDesc = renderingData.cameraData.cameraTargetDescriptor;

        if (material != null)
        {
            int rtW = rtDesc.width / downSample;
            int rtH = rtDesc.height / downSample;

            Debug.Log(string.Format("rtW:[{0}],  rtH:[{1}]", rtW, rtH));

            RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
            buffer0.filterMode = FilterMode.Bilinear;

            cmd.Blit(cameraTexture, buffer0);

            for (int i = 0; i < iterations; i++)
            {
                material.SetFloat("_BlurSize", 1.0f + i * blurSpread);
                RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // 水平通道
                cmd.Blit(buffer0, buffer1, material, 0);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
                buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                // 垂直通道
                cmd.Blit(buffer0, buffer1, material, 1);
                RenderTexture.ReleaseTemporary(buffer0);
                buffer0 = buffer1;
            }

            cmd.Blit(buffer0, cameraTexture);
            RenderTexture.ReleaseTemporary(buffer0);
        }
    }
}