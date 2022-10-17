using System;

namespace UnityEngine.Rendering.Universal
{
    [Serializable, VolumeComponentMenuForRenderPipeline("Post-processing/Bloom", typeof(UniversalRenderPipeline))]
    public sealed class GaussianBlur : VolumeComponent, IPostProcessComponent
    {
        [Header("GaussianBlur")]
        // 模糊迭代越大越模糊。
        [Range(0, 4)]
        public int iterations = 3;
	
        // 模糊扩展，越大越多的模糊
        [Range(0.2f, 3.0f)]
        public float blurSpread = 0.6f;
	
        [Range(1, 8)]
        public int downSample = 2;
      
        public bool IsActive() => blurSpread > 0f;
        //
        public bool IsTileCompatible() => false;
    }
}
