using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtmosphereEffect
{
    Light lightSource;
    public Material mat { get; private set; }

    public void UpdateSettings(PlanetaryBody planet)
    {
        Shader shader = planet.atmosphereSettings.atmosphereShader;
        if(mat == null || mat.shader != shader)
        {
            mat = new Material(shader);
            planet.atmosphereSettings.settingsUpToDate = false;
        }
        planet.atmosphereSettings.SetProperties(mat, planet.planetRadius, planet.planetAtmosphere, planet.updateAtmosphere);
        if (lightSource == null)
            lightSource = GameObject.FindObjectOfType<Light>()?.GetComponent<Light>();

        mat.SetVector("planetCentre", planet.transform.position);
        if (lightSource)
            mat.SetVector("dirToSun", lightSource.transform.forward * -1);
        else
            mat.SetVector("dirToSun", Vector3.up);
    }
}
