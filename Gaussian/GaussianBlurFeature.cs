using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GaussianBlurFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class GaussianBlurSettings
    {
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
        public Material material = null;
        public Shader shader = null;

        // 模糊迭代越大越模糊。
        [Range(0, 4)] public int iterations = 3;

        // 模糊扩展，越大越多的模糊
        [Range(0.2f, 3.0f)] public float blurSpread = 0.6f;

        [Range(1, 8)] public int downSample = 2;
    }

    private GaussianBlurPostPass postPass;
    public GaussianBlurSettings settings = new GaussianBlurSettings();

    public override void Create()
    {
        postPass = new GaussianBlurPostPass();
        postPass.renderPassEvent = settings.renderPassEvent;
        postPass.shader = settings.shader;
        // Material material = CoreUtils.CreateEngineMaterial(settings.shader);
        postPass.material = settings.material;
        postPass.iterations = settings.iterations;
        postPass.blurSpread = settings.blurSpread;
        postPass.downSample = settings.downSample;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        renderer.EnqueuePass(postPass);
    }
}