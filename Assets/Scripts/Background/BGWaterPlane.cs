using UnityEngine;

public class BGWaterPlane : MonoBehaviour
{
    private Material _material;

    private void Awake()
    {
        _material = GetComponent<MeshRenderer>().material;

        Events.OnChangeBGColor += ChangeBGColor;
    }

    void ChangeBGColor(Color color)
    {
        // Modify these variables to change the texture
        int textureWidth = 256;
        float[] colorPositions = new float[] {0, 0.01f, 0.15f, 1};
        float[] colorIntensities = new float[] {2, 1, 0.75f, 0.1f};


        // Creating colors
        Color[] colors = new Color[colorPositions.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            float intensity = colorIntensities[i];
            colors[i] = new Color(color.r * intensity, color.g * intensity, color.b * intensity);
        }

        // Creating a gradient
        Gradient gradient = new();

        GradientColorKey[] colorKeys = new GradientColorKey[colorPositions.Length];
        for (int i = 0; i < colorKeys.Length; i++)
        {
            colorKeys[i] = new GradientColorKey(colors[i], colorPositions[i]);
        }

        gradient.SetKeys(colorKeys, new GradientAlphaKey[] {});


        // Creating a texture
        Texture2D gradientTexture = new(textureWidth, 1, TextureFormat.RGBA32, false);

        for (int i = 0; i < textureWidth; i++)
        {
            Color pixelColor = gradient.Evaluate((float)i / textureWidth);
            gradientTexture.SetPixel(i, 0, pixelColor);
        }

        gradientTexture.wrapMode = TextureWrapMode.Clamp;

        gradientTexture.Apply();


        _material.SetTexture("_WaterGradientTexture", gradientTexture);
    }
    
    private void OnDestroy()
    {
        Events.OnChangeBGColor -= ChangeBGColor;
    }
}
