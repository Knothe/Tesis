using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "AtmosphereSettings")]
public class AtmosphereSettings : ScriptableObject
{
    public Material m;
    public Shader atmosphereShader { get { return m.shader; } }
    public ComputeShader opticalDepthCompute;
    public int textureSize = 256;
    public int inScatteringPoints = 10;
    public int opticalDepthPoints = 10;


    public Vector3 wavelengths = new Vector3(700, 530, 460);
    public Vector4 testParams = new Vector4(7, 1.26f, .1f, 3);

    public AtmosphereValues[] atmosphereValues = new AtmosphereValues[] {new AtmosphereValues(), new AtmosphereValues()};

    public float ditherStrength = 0.8f;
    public float ditherScale = 4;
    [Range(0, 1)]
    public float atmosphereScale = .5f;


    public Texture2D blueNoise;

    RenderTexture opticalDepthTexture;
    public bool settingsUpToDate { get; set; }
    int index;
    bool lastAtmosphere;

    public void SetProperties(Material material, float radius, bool atmosphere, bool updateAtmosphere)
    {
        
        if(!settingsUpToDate || !Application.isPlaying || updateAtmosphere)
        {
            lastAtmosphere = atmosphere;
            if (atmosphere)
                index = 0;
            else
                index = 1;

            float atmosphereRadius = radius * (1 + atmosphereScale); // CAMBIAR

            material.SetVector("params", testParams);
            material.SetInt("numInScatteringPoints", inScatteringPoints);
            material.SetInt("numOpticalDepthPoints", opticalDepthPoints);
            material.SetFloat("atmosphereRadius", atmosphereRadius);
            material.SetFloat("planetRadius", radius);
            material.SetFloat("densityFalloff", atmosphereValues[index].densityFalloff);

            // Strength of (rayleigh) scattering is inversely proportional to wavelength^4
            float scatterX = Mathf.Pow(400 / wavelengths.x, 4);
            float scatterY = Mathf.Pow(400 / wavelengths.y, 4);
            float scatterZ = Mathf.Pow(400 / wavelengths.z, 4);
            material.SetVector("scatteringCoefficients", new Vector3(scatterX, scatterY, scatterZ) * atmosphereValues[index].scatteringStrength);
            material.SetFloat("intensity", atmosphereValues[index].intensity);
            material.SetFloat("ditherStrength", ditherStrength);
            material.SetFloat("ditherScale", ditherScale);
            material.SetTexture("_BlueNoise", blueNoise);

            PrecomputeOutScattering();
            material.SetTexture("_BakedOpticalDepth", opticalDepthTexture);

            settingsUpToDate = true;
        }

    }

    void PrecomputeOutScattering()
    {
        if (opticalDepthTexture == null || !opticalDepthTexture.IsCreated())
        {
            ComputeHelper.CreateRenderTexture(ref opticalDepthTexture, textureSize, FilterMode.Bilinear);
            opticalDepthCompute.SetTexture(0, "Result", opticalDepthTexture);
            opticalDepthCompute.SetInt("textureSize", textureSize);
            opticalDepthCompute.SetInt("numOutScatteringSteps", opticalDepthPoints);
            opticalDepthCompute.SetFloat("atmosphereRadius", (1 + atmosphereScale));
            opticalDepthCompute.SetFloat("densityFalloff", atmosphereValues[index].densityFalloff);
            opticalDepthCompute.SetVector("params", testParams);
            ComputeHelper.Run(opticalDepthCompute, textureSize, textureSize);
        }
    }

    private void OnValidate()
    {
        settingsUpToDate = false;
    }

    [System.Serializable]
    public class AtmosphereValues
    {
        public float densityFalloff = 0.25f;
        public float scatteringStrength = 20;
        public float intensity = 1;
    }
}
