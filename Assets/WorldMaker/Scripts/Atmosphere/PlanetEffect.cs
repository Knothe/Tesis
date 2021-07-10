using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu (menuName = "PlanetEffect")]
public class PlanetEffect : ScriptableObject
{

    public Shader atmosphereShader;
    public bool displayAtmosphere = true;

    List<EffectHolder> effectHolders;
    List<Material> postProcessingMaterials;

    public void SetPlanetsList()
    {
        PlanetaryBody[] planets = FindObjectsOfType<PlanetaryBody>();
        effectHolders = new List<EffectHolder>(planets.Length);
        foreach (PlanetaryBody p in planets)
            effectHolders.Add(new EffectHolder(p));
    }

    public void Render(RenderTexture source, RenderTexture destination)
    {
        GetMaterials();
        CustomPostProcessing.RenderMaterials(source, destination, postProcessingMaterials);
    }

    void Init()
    {
        if(effectHolders == null || effectHolders.Count == 0 || !Application.isPlaying)
        {
            PlanetaryBody[] planets = FindObjectsOfType<PlanetaryBody>();
            effectHolders = new List<EffectHolder>(planets.Length);

            foreach (PlanetaryBody p in planets)
                effectHolders.Add(new EffectHolder(p));
        }

        if (postProcessingMaterials == null)
            postProcessingMaterials = new List<Material>();

        postProcessingMaterials.Clear();
    }

    void GetMaterials()
    {
        Init();

        if(effectHolders.Count > 0)
        {
            Camera cam = Camera.current;
            Vector3 camPos = cam.transform.position;

            SortFarToNear(camPos);

            if (displayAtmosphere)
            {
                for (int i = 0; i < effectHolders.Count; i++)
                {
                    if (effectHolders[i].atmosphereEffect != null)
                    {
                        effectHolders[i].UpdateSettings();
                        postProcessingMaterials.Add(effectHolders[i].atmosphereEffect.mat);
                    }
                }
            }
        }
    }

    void SortFarToNear(Vector3 viewPos)
    {
        foreach (EffectHolder e in effectHolders)
            e.SetDstFromSurface(viewPos);

        EffectHolder temp;
        for(int i = 0; i < effectHolders.Count - 1; i++)
        {
            for (int j = i + 1; j > 0; j--)
            {
                if (effectHolders[j - 1].distance < effectHolders[j].distance)
                {
                    temp = effectHolders[j - 1];
                    effectHolders[j - 1] = effectHolders[j];
                    effectHolders[j] = temp;
                }
            }
        }
    }

    public class EffectHolder
    {
        public PlanetaryBody planet { get; private set; }
        public AtmosphereEffect atmosphereEffect;
        public float distance { get; private set; }

        public EffectHolder(PlanetaryBody p)
        {
            planet = p;
            atmosphereEffect = new AtmosphereEffect();
        }

        public void UpdateSettings()
        {
            atmosphereEffect.UpdateSettings(planet);
        }

        public void SetDstFromSurface(Vector3 viewPos)
        {
            distance = Mathf.Max(0, (planet.transform.position - viewPos).magnitude - (planet.planetRadius + planet.planetHeight)); // Tal vez modificar
        }
    }


}


