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
    [SerializeField] private float targetFrequency = 1.5f;
    [SerializeField]
    private float targetMovementSpeed = 1f;
    [Range(0.1f, 1f)]
    public float smoothTime = 0.5f; // tiempo que tarda en alcanzar el valor
    private float freqVelocity = 0f;
    private float speedVelocity = 0f;
    [SerializeField] Player player;

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
        player.OnMakeSound += MakeSound;
        player.OnMakeImpactSound += MakeImpactSound;
        player.OnHpChange += ChangeColorWave;
    }

    void Draw()
    {
        float step = frequency / (float)points;
        float xStart = xLimits.x;
        float Tau = 2 * Mathf.PI;
        float xFinish = xLimits.y;

        if (texture != null)
            texture.SetPixels(colors);

        for (int currentPoint = 0; currentPoint < points; currentPoint++)
        {
            float sample = PerlinNoiseOctaves(currentPoint, 5, step, (int)movementSpeed);
            float currentAmplitude = (Mathf.Clamp01(frequency / 8f) * amplitude);
            sample = ((sample - .5f) * currentAmplitude * 0.5f) + amplitude * 0.5f;
            int y = sample < 0 ? 1 : sample > amplitude ? amplitude-1 : (int) sample;
            texture.SetPixel(currentPoint, y-1, color);
            texture.SetPixel(currentPoint, y, color);
            texture.SetPixel(currentPoint, y+1, color);
        }
        if (texture != null)
            texture.Apply();
    }

    void Update()
    {
        //funcion que hace transiciones suaves, se le pasa el valor actual, el objetivo y cuanto tiempo tarda en llegar (smoothTime).
        frequency = Mathf.SmoothDamp(frequency, targetFrequency, ref freqVelocity, smoothTime);
        movementSpeed = Mathf.SmoothDamp(movementSpeed, targetMovementSpeed, ref speedVelocity, smoothTime);
        Draw();
    }


    private float PerlinNoiseOctaves(float x, float y, float step, int octaves = 0)
    {
        float xCoord = (Time.timeSinceLevelLoad/*frequency*.5f*/) + x * step;
        float yCoord = y;
        float result = Mathf.PerlinNoise(xCoord, yCoord);

        for (int octave = 1; octave < octaves; octave++)
        {
            float newStep = step * 2 * octave;
            float xOctaveCoord = (Time.timeSinceLevelLoad/*frequency*.5f*/) + x * newStep;
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
        targetFrequency = noiseIntensity;
        if (targetFrequency<1) targetFrequency = 1;
        targetMovementSpeed = 1f;

        if (noiseIntensity > 6)
        {
            targetMovementSpeed += noiseIntensity / 2f;
        }
    }

    private float currentTargetFrequency;
    private float currentTargetMovementSpeed;
    private float currentSmoothTime;
    public void MakeImpactSound(int noiseIntensity)
    {
        StartCoroutine(ImpactCoroutine(currentTargetFrequency, currentTargetMovementSpeed, noiseIntensity));
    }

    private IEnumerator ImpactCoroutine(float currentTargetFrequency, float currentTargetMovementSpeed, int noiseIntensity)
    {
        currentTargetMovementSpeed = this.targetMovementSpeed;
        currentTargetFrequency = this.targetFrequency;
        currentSmoothTime = this.smoothTime;

        this.smoothTime = 0.1f;
        this.targetMovementSpeed = noiseIntensity;
        this.targetFrequency = noiseIntensity;
        yield return new WaitForSeconds(0.5f);
        this.targetFrequency = currentTargetFrequency;
        if (targetFrequency < 1) targetFrequency = 1;
        this.targetMovementSpeed = currentTargetMovementSpeed;
    }

    private void ChangeColorWave(int hp, int maxHp)
    {
        if (hp <= 4 && hp > 2)
        {
            color = Color.yellow;
        }
        else if (hp <= 2)
        {
           color = Color.red;
        }
        else
        {
            color = Color.green;
        }
    }

    private void OnDestroy()
    {
        player.OnMakeImpactSound -= MakeImpactSound;
        player.OnMakeSound -= MakeSound;
    }

}
