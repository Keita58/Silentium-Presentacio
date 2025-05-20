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
                First = true;
            }
        }
        else if (!Second)
        {
            if (scores[1] > 0.8f)
            {
                Second = true;
            }
        }
        else if (!Third)
        {
            if (scores[2] > 0.8f)
            {
                Third = true;
            }
        }
        else if (!Fourth)
        {
            if (scores[3] > 0.8f)
            {
                Fourth = true;
            }
        }
        else if (!Fifth)
        {
            if (scores[1] > 0.8f)
            {
                Fifth = true;
            }
        }
        else if (!Sixth)
        {
            if (scores[0] > 0.8f)
            {
                Sixth = true;
            }
        }
        else if (!Seventh)
        {
            if (scores[4] > 0.8f)
            {
                Seventh = true;
            }
        }
        else if (!Eighth)
        {
            if (scores[5] > 0.8f)
            {
                Eighth = true;
                PuzzleManager.instance.HieroglyphicPuzzleExit(true);
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