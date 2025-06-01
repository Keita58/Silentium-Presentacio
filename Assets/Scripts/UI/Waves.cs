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
        //Step és la freqüència entre l'amplada de la textura (points és amplada).
        float step = frequency / (float)points;
        if (texture != null)
            texture.SetPixels(colors);

        //Recorrem l'amplada de la textura i per cada punt calculem el valor de la funció de Perlin.
        for (int currentPoint = 0; currentPoint < points; currentPoint++)
        {
            float sample = PerlinNoiseOctaves(currentPoint, 5, step, (int)movementSpeed);
            //Escalem l'amplitud segons la freqüència i l'amplitud màxima (Això es va fer per als canvis sobtats com trets). Vam posar 8 perquè quedés bé, més gran pot ser massa.
            float currentAmplitude = (Mathf.Clamp01(frequency / 8f) * amplitude);
            //Centrem l'ona verticalment a la texura
            sample = ((sample - .5f) * currentAmplitude * 0.5f) + amplitude * 0.5f;
            //Ajustem el valor de la mostra perquè no surti de l'amplitud màxima.
            int y = sample < 0 ? 1 : sample > amplitude ? amplitude-1 : (int) sample;
            
            //Pintem tres píxels verticals per cada punt de la textura per fer l'ona més visible, més gruixuda.
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
        //Això fa que l'ona és mogui horitzontalment amb el temps.
        float xCoord = Time.timeSinceLevelLoad + x * step;
        //El valor de la y no es toca
        float yCoord = y;
        //Fem la mostra de Perlin amb les coordenades x i y.
        float result = Mathf.PerlinNoise(xCoord, yCoord);

        //Per cada octava augmentem el valor de la freqüència i afegim una mostra de Perlin amb un valor més baix.
        for (int octave = 1; octave < octaves; octave++)
        {
            //Augmentem la freqüència de l'octava, per exemple, si l'octava és 1, la freqüència serà el doble de la inicial.
            float newStep = step * 2 * octave;
            //Tornem a calcular les coordenades x i y per a l'octava.
            float xOctaveCoord = Time.timeSinceLevelLoad + x * newStep;
            float yOctaveCoord = y;
            //Tornem a fer un perlin amb les noves coordenades
            float octaveSample = Mathf.PerlinNoise(xOctaveCoord, yOctaveCoord);
            //Augmentem la freqüència de l'octava i es redueix l'amplitud del perlin a cada octava.
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
        StartCoroutine(ImpactCoroutine(noiseIntensity));
    }

    private IEnumerator ImpactCoroutine(int noiseIntensity)
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
