using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode, ImageEffectAllowedInSceneView]
public class CustomPostProcessing : MonoBehaviour
{
    public PlanetEffect effect;
    Shader defaultShader;
    Material defaultMat;
    List<RenderTexture> temporaryTextures = new List<RenderTexture>();

    public event System.Action<RenderTexture> onPostProcessingComplete;
    public event System.Action<RenderTexture> onPostProcessingBegin;

    private void Start()
    {
        effect.SetPlanetsList();
    }

    private void Init()
    {
        if(defaultShader == null)
        {
            defaultShader = Shader.Find("Unlit/Texture");
        }

        if (temporaryTextures == null)
            temporaryTextures = new List<RenderTexture>();
        temporaryTextures.Clear();

        defaultMat = new Material(defaultShader);
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (onPostProcessingBegin != null)
            onPostProcessingBegin(destination);
        Init();

        RenderTexture currentSource = source;
        RenderTexture currentDestination = destination;

        if(effect != null)
        {
            effect.Render(currentSource, currentDestination);
            currentSource = currentDestination;
        }

        if (currentDestination != destination)
            Graphics.Blit(currentSource, destination, defaultMat);

        if (onPostProcessingComplete != null)
            onPostProcessingComplete(destination);
    }


    public static void RenderMaterials(RenderTexture source, RenderTexture destination, List<Material> materials)
    {
        List<RenderTexture> temporaryTextures = new List<RenderTexture>();

        RenderTexture currentSource = source;
        RenderTexture currentDestination = null;

        if(materials != null)
        {
            for (int i = 0; i < materials.Count; i++)
            {
                Material material = materials[i];
                if(material != null)
                {
                    if (i == materials.Count - 1)
                        currentDestination = destination;
                    else
                    {
                        currentDestination = TemporaryRenderTexture(destination);
                        temporaryTextures.Add(currentDestination);
                    }
                    Graphics.Blit(currentSource, currentDestination, material);
                    currentSource = currentDestination;
                }
            }
        }

        if(currentDestination != destination)
            Graphics.Blit(currentSource, destination, new Material(Shader.Find("Unlit/Texture")));

        for (int i = 0; i < temporaryTextures.Count; i++)
            RenderTexture.ReleaseTemporary(temporaryTextures[i]);
    }

    public static RenderTexture TemporaryRenderTexture(RenderTexture template)
    {
        return RenderTexture.GetTemporary(template.descriptor);
    }
}
