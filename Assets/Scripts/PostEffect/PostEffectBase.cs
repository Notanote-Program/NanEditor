using UnityEngine;
using System.Collections;
using System.Diagnostics.Contracts;

[ExecuteInEditMode]//这个特性意为在编辑器模式下也可以运行
[RequireComponent(typeof(Camera))] //绑定Camera；
public class PostEffectBase : MonoBehaviour
{
    private Material _material = null;
    public Shader shader;
    public Material material
    {
        get
        {
            _material = CheckShaderAndCreateMaterial(shader, _material); //指明Shader和材质
            return _material;
        }
    }

    /// <summary>
    /// 检查shader并创建材质 
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="material"></param>
    /// <returns></returns>
    /// 参数是shader 和 material
    protected Material CheckShaderAndCreateMaterial(Shader shader, Material material)
    {
        if (shader == null)
        {
            return null;
        }

        if (shader.isSupported && material && material.shader == shader)
            return material;

        if (!shader.isSupported)
        {
            return null;
        }
        else
        {
            material = new Material(shader);
            material.hideFlags = HideFlags.DontSave;
            if (material)
                return material;
            else
                return null;
        }
    }
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        if (material != null)
        {
            //用Graphics.Blit绘制
            Graphics.Blit(src, dest, material);
        }
        else
        {
            //要是没有Material 就初始化一下
            Graphics.Blit(src, dest);
        }
    }
}