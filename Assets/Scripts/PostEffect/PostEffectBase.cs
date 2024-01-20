using UnityEngine;
using System.Collections;
using System.Diagnostics.Contracts;

[ExecuteInEditMode]//���������Ϊ�ڱ༭��ģʽ��Ҳ��������
[RequireComponent(typeof(Camera))] //��Camera��
public class PostEffectBase : MonoBehaviour
{
    private Material _material = null;
    public Shader shader;
    public Material material
    {
        get
        {
            _material = CheckShaderAndCreateMaterial(shader, _material); //ָ��Shader�Ͳ���
            return _material;
        }
    }

    /// <summary>
    /// ���shader���������� 
    /// </summary>
    /// <param name="shader"></param>
    /// <param name="material"></param>
    /// <returns></returns>
    /// ������shader �� material
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
            //��Graphics.Blit����
            Graphics.Blit(src, dest, material);
        }
        else
        {
            //Ҫ��û��Material �ͳ�ʼ��һ��
            Graphics.Blit(src, dest);
        }
    }
}