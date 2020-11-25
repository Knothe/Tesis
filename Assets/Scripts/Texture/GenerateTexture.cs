using Unity.Mathematics;
using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
public class GenerateTexture : MonoBehaviour
{
    MeshRenderer meshRenderer;
    Material material;
    Texture2D texture;
    public Color color1;
    public Color color2;
    public Color color3;
    public float3 limits;

    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        material = meshRenderer.material;
        texture = new Texture2D(256, 256, TextureFormat.RGBA32, true, true);
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.filterMode = FilterMode.Bilinear;
        material.SetTexture("_MainTex", texture);
        CreateTexture();
    }

    void CreateTexture()
    {
        Color temp = Color.white;
        float value;
        for(int i = 0; i < texture.height; i++)
        {
            for(int j = 0; j < texture.width; j++)
            {
                value = UnityEngine.Random.Range(0, limits.z);
                if (value < limits.x)
                    temp = color1;
                else if (value < limits.y)
                    temp = color2;
                else
                    temp = color3;
                texture.SetPixel(i, j, temp);
            }
        }
        texture.Apply();
    }

    

    void Update()
    {
        
    }
}
