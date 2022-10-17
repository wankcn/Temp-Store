using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class AdditionPostProcessPass : ScriptableRenderPass
{
    //1.创建所需属性

    //标签名，用于续帧调试器中显示缓冲区名称
    const string CommandBufferTag = "MyPostProcessing Pass";

    // 用于后处理的材质
    public Material m_Material;

    // 属性参数组件
    GaussianBlur m_UseGaussianURP;

    // 颜色渲染标识符
    RenderTargetIdentifier src;
    // 临时的渲染目标
    RenderTargetHandle m_TemporaryTexture;
    
	public AdditionPostProcessPass()
    {          
        m_TemporaryTexture.Init("_TemporaryColorTexture1");
    }
    //2.创一个入口函数，用于后续渲染管线功能脚本写入参数

    // 设置渲染参数
    public void Setup(RenderTargetIdentifier src, Material Material)
    {
        this.src = src;

        m_Material = Material;
    }

    //3.修改Execute方法，在方法中我们通过VolumeManager.instance.stack单例获取Volume框架中所有的堆栈，在从堆栈中获取我们上一部创建的属性参数组件，由于Execute是每帧调用，所有该组件也是实时更新的。

    //然后我们使用标签名获取一个命令缓冲区，将该命令缓冲区与Execute的参数RenderingData渲染信息一起交给Render方法进行后处理。

    //在后处理完成后我们调用context.ExecuteCommandBuffer(cmd);方法执行该命令缓冲区，最后释放内存。

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        // 从Volume框架中获取所有堆栈
        var stack = VolumeManager.instance.stack;
        // 从堆栈中查找对应的属性参数组件
        m_UseGaussianURP = stack.GetComponent<GaussianBlur>();

        // 从命令缓冲区池中获取一个带标签的命令缓冲区，该标签名可以在后续帧调试器中见到
        var cmd = CommandBufferPool.Get(CommandBufferTag);

        // 调用渲染函数
        Render(cmd, ref renderingData);

        // 执行命令缓冲区
        context.ExecuteCommandBuffer(cmd);
        // 释放命令缓存
        CommandBufferPool.Release(cmd);
        // 释放临时RT
        cmd.ReleaseTemporaryRT(m_TemporaryTexture.id);
    }

    //编写渲染方法Render,在Render方法中我们获取属性参数组件中的参数，赋值给材质。

    //然后通过RenderingData对象中的相机信息创建一个临时缓冲区，然后将颜色渲染器中的颜色通过Shader进行计算后保存到临时缓冲区中。

    //最后在从临时缓冲区中读取出来返还到主纹理中。

    private void Render(CommandBuffer cmd, ref RenderingData renderingData)
    {
        // VolumeComponent是否开启，且非Scene视图摄像机
        if (m_UseGaussianURP.IsActive() && !renderingData.cameraData.isSceneViewCamera)
        {
            
            // 获取目标相机的描述信息
            RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;

            if (m_Material != null) {
                int rtW = opaqueDesc.width/m_UseGaussianURP.downSample;
                int rtH = opaqueDesc.height/m_UseGaussianURP.downSample;

                RenderTexture buffer0 = RenderTexture.GetTemporary(rtW, rtH, 0);
                buffer0.filterMode = FilterMode.Bilinear;

                cmd.Blit(src, buffer0);

                for (int i = 0; i < m_UseGaussianURP.iterations; i++) {
                    m_Material.SetFloat("_BlurSize", 1.0f + i * m_UseGaussianURP.blurSpread);

                    RenderTexture buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                    // 水平通道
                    cmd.Blit(buffer0, buffer1, m_Material, 0);

                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                    buffer1 = RenderTexture.GetTemporary(rtW, rtH, 0);

                    // 垂直通道
                    cmd.Blit(buffer0, buffer1, m_Material, 1);

                    RenderTexture.ReleaseTemporary(buffer0);
                    buffer0 = buffer1;
                }
                
                cmd.Blit(buffer0, src);
                RenderTexture.ReleaseTemporary(buffer0);
                
            } 

            
            // 写入参数
            // m_Material.SetFloat("_Strength", m_UseGaussianURP.strength.value);
            
            // 获取目标相机的描述信息
            // RenderTextureDescriptor opaqueDesc = renderingData.cameraData.cameraTargetDescriptor;
            
            // 设置深度缓冲区
            // opaqueDesc.depthBufferBits = 0;
            //
            // // 通过目标相机的渲染信息创建临时缓冲区
            // cmd.GetTemporaryRT(m_TemporaryColorTexture01.id, opaqueDesc);
            //
            // // 通过材质，将计算结果存入临时缓冲区
            // cmd.Blit(m_ColorAttachment, m_TemporaryColorTexture01.Identifier(), m_Material);
            // // 再从临时缓冲区存入主纹理
            // cmd.Blit(m_TemporaryColorTexture01.Identifier(), m_ColorAttachment);
        }
    }
    
}
