//
// @Description: 与场景后处理有关的功能方法
// @Author: 文若
// @CreateDate: 2022-10-19
// 

using UnityEngine;

namespace GameNeon.Utils
{
    public class PostEffectUtil
    {
        private const string _BlurSize = "_BlurSize";
        private static readonly string GaussianBlurEffectShader = "Hidden/BlurImageEffect";
        
        
        /// <summary>
        /// RT高斯模糊渲染 需要配置Shader路径
        /// </summary>
        /// <param name="src"></param>
        /// <param name="downSample">降采样系数</param>
        /// <param name="iterations">迭代次数</param>
        /// <param name="blurSpread">模糊范围</param>
        /// <returns></returns>
        public static RenderTexture GaussianRenderImage(RenderTexture src, int downSample = 2, int iterations = 3,
            float blurSpread = 0.6f)
        {
            Material mat = new Material(Shader.Find(GaussianBlurEffectShader));
            RenderTexture dest = new RenderTexture(src.width, src.height, 0);
            if (mat != null)
            {
                int rtW = src.width / downSample;
                int rtH = src.height / downSample;
                RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
                buffer0.filterMode = FilterMode.Bilinear;
                Graphics.Blit(src, buffer0);
                for (int i = 0; i < iterations; i++)
                {
                    mat.SetFloat(_BlurSize, 1.0f + i * blurSpread);
                    RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                    Graphics.Blit(buffer0, buffer1, mat, 0);
                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                    buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);
                    Graphics.Blit(buffer0, buffer1, mat, 1);
                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                }

                Graphics.Blit(buffer0, dest);
                RenderTexture.ReleaseTemporary(buffer0);
                return dest;
            }

            return null;
        }
    }
}