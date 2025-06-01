using UnityEngine;
using Unity.Barracuda;
using System.IO;

public class AI : MonoBehaviour
{
    [SerializeField]
    private Camera drawCamera;
    [SerializeField]
    private RenderTexture renderTexture;
    [SerializeField]
    private NNModel modelAsset;
    private Model runtimeModel;
    private IWorker worker;
    [SerializeField]
    private bool First = false;
    [SerializeField]
    private bool Second = false;
    [SerializeField] 
    private bool Third = false;
    [SerializeField] 
    private bool Fourth = false;
    [SerializeField] 
    private bool Fifth = false;
    [SerializeField] 
    private bool Sixth = false;
    [SerializeField] 
    private bool Seventh = false;
    [SerializeField] 
     private bool Eighth = false;
    [Header("Audio")]
    AudioSource audioSource;
    [SerializeField]
    AudioClip correct;
    [SerializeField]
    AudioClip incorrect;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }
    void Start()
    {
        runtimeModel = ModelLoader.Load(modelAsset);
        worker = WorkerFactory.CreateWorker(WorkerFactory.Type.Auto, runtimeModel);
    }
    public Texture2D CaptureDrawing()
    {
        RenderTexture currentRT = RenderTexture.active;
        RenderTexture.active = renderTexture;

        Texture2D image = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.RGBA32, false);
        image.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
        float threshold = 0.05f; //Posem aquest threshold per tal que reconeixi les linies i no el fons gris

        for (int x = 0; x < image.width; x++)
        {
            for (int y = 0; y < image.height; y++)
            {
                Color pixel = image.GetPixel(x, y);

                //Calculem la brillantor del pixel i mirem si es mes petit que el threshold, per tant el considerem negre
                float luminance = 0.2126f * pixel.r + 0.7152f * pixel.g + 0.0722f * pixel.b;

                //Si es obscur el pixel, el considerem negre si no blanc
                image.SetPixel(x, y, luminance < threshold ? Color.black : Color.white);
            }
        }
        image.Apply();

        RenderTexture.active = currentRT;
        byte[] bytes = image.EncodeToPNG();
        File.WriteAllBytes(Application.persistentDataPath + "/Captured.png", bytes);
        Classify(image);
        return image;
    }
    public void Classify(Texture2D inputImage)
    {
        
        Tensor input = new Tensor(inputImage, 3); // 3 = RGB
        worker.Execute(input);

        Tensor output = worker.PeekOutput();
        float[] scores = output.ToReadOnlyArray();

        for (int i = 0; i < scores.Length; i++)
        {
            Debug.Log($"Clase {i+1}: {scores[i]}");
        }
        if (!First)
        {
            Debug.Log("Er 0:" + scores[0]);
            if (scores[0] > 0.6f)
            {
                audioSource.PlayOneShot(correct);
                First = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Second)
        {
            if (scores[1] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Second = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Third)
        {
            if (scores[2] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Third = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Fourth)
        {
            if (scores[3] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Fourth = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Fifth)
        {
            if (scores[1] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Fifth = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Sixth)
        {
            if (scores[0] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Sixth = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Seventh)
        {
            if (scores[4] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Seventh = true;
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        else if (!Eighth)
        {
            if (scores[5] > 0.8f)
            {
                audioSource.PlayOneShot(correct);
                Eighth = true;
                PuzzleManager.instance.HieroglyphicPuzzleExitAnimation();
            }
            else
            {
                audioSource.PlayOneShot(incorrect);
            }
        }
        input.Dispose();
        output.Dispose();
    }

    void OnDestroy()
    {
        worker.Dispose();
    }
}