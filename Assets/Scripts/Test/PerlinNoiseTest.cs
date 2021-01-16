using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class PerlinNoiseTest : MonoBehaviour
{
    public int width = 256;
    public int height = 256;

    public float scale1;
    public Vector2 offset1;
    public float scale2;
     public Vector2 offset2;

    public Color[] TypeOfTree = new Color[4];

    public Renderer r;

    void Update()
    {
       r.material.mainTexture = GenerateTexture();
    }

    Texture2D GenerateTexture()
    {
        Texture2D texture = new Texture2D(width, height);

        int id;
        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < height; y++)
            {
                id = 0;
                if (CalculateColor(x, y, scale1, offset1))
                    id += 1;
                if (CalculateColor(x, y, scale2, offset2))
                    id += 2;
                texture.SetPixel(x, y, TypeOfTree[id]);
            }
        }
        texture.Apply();
        return texture;
    }

    bool CalculateColor(int x, int y, float scale, Vector2 offset)
    {
        float xCoord = (float)x * scale;
        float yCoord = (float)y / 10 * scale;
        float sample = Mathf.PerlinNoise(xCoord + offset.x, yCoord + offset.y);
        return sample < .5f;
    }

}
