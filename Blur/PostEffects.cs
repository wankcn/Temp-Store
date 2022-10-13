using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostEffects : PostEffectsBase
{
    [Range(0, 16)] public int times = 1;

    void Start()
    {
        Check();
    }

    void OnRenderImage(RenderTexture src, RenderTexture des)
    {
        if (effect_enable)
        {
            RenderTexture tmp1 = new RenderTexture(src.width, src.height, src.depth);
            RenderTexture tmp2 = new RenderTexture(src.width, src.height, src.depth);
            
            // 使用着色器将源纹理复制到目标渲染纹理
            Graphics.Blit(src, tmp1);
            // 混合结果再次混合
            for (var i = 0; i < times; i++)
            {
                Graphics.Blit(tmp1, tmp2, material);
                Graphics.Blit(tmp2, tmp1);
            }

            Graphics.Blit(tmp1, des);

            tmp1.Release();
            tmp2.Release();
        }
        else
        {
            Graphics.Blit(src, des);
        }
    }
}