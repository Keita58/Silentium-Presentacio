using System.Collections;
using UnityEngine;
using UnityEngine.UI;
 
public class Waves : MonoBehaviour
{
    public int points;
    public int amplitude = 1;
    public float frequency = 0;
    public Vector2 xLimits = new Vector2(0, 1);
    [Range(1, 8f)]
    public float movementSpeed = 1;
    [SerializeField] GameObject Imatge;
    Texture2D texture;
    Color[] colors;
    private Color color = Color.green;

    void Start()
    {
        points = (int)Imatge.GetComponent<RawImage>().rectTransform.rect.width;
        amplitude = (int)Imatge.GetComponent<RawImage>().rectTransform.rect.height;

        texture = new Texture2D(points, amplitude);
        texture.filterMode = FilterMode.Point;
        texture.alphaIsTransparency = true;
        Imatge.GetComponent<RawImage>().texture = texture;

        colors = new Color[points * amplitude];
        Color transparent = new Color(0, 0, 0, 0);
        for (int i = 0; i < colors.Length; i++)
            colors[i] = transparent;
    }

    void Draw()
    {
        float step = frequency / (float)points;
        float xStart = xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = xLimits.y;

        texture.SetPixels(colors);

        for (int currentPoint = 0; currentPoint < points; currentPoint++)
        {
            
            float sample = PerlinNoiseOctaves(currentPoint, 5, step, (int)movementSpeed);
            int y = (int)(amplitude*sample);
            y = y == 0 ? 1 : y;
            texture.SetPixel(currentPoint, y-1, color);
            texture.SetPixel(currentPoint, y, color);
            texture.SetPixel(currentPoint, y+1, color);
        }
        texture.Apply();
    }
    void Update()
    {
        Draw();
    }

    private float PerlinNoiseOctaves(float x, float y, float step, int octaves = 0)
    {
        float xCoord = (Time.timeSinceLevelLoad*frequency*.5f) + x * step;
        float yCoord = y;
        float result = Mathf.PerlinNoise(xCoord, yCoord);

        for (int octave = 1; octave < octaves; octave++)
        {
            float newStep = step * 2 * octave;
            float xOctaveCoord = (Time.timeSinceLevelLoad*frequency*.5f) + x * newStep;
            float yOctaveCoord = y;

            float octaveSample = Mathf.PerlinNoise(xOctaveCoord, yOctaveCoord);
            octaveSample = (octaveSample - .5f ) * (.8f / octave);

            result += octaveSample;
        }

        return Mathf.Clamp01(result);
    }

    public void SetColor(Color colorToSet)
    {
        color = colorToSet;
    }

    public void MakeSound(int noiseIntensity)
    {
        float currentFrequency = 0f;
        float currentMovementSpeed = 1f;
        currentFrequency += noiseIntensity;
        frequency = currentFrequency;
        if (noiseIntensity > 6)
        {
            currentMovementSpeed += noiseIntensity / 2f;
        }
        movementSpeed = currentMovementSpeed;


    }
}
