// Programatically add a LineRenderer component and draw a 3D line.

using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

public class LineRendererExample : MonoBehaviour
{
    [SerializeField]
    private AI ocr;
    [SerializeField]
    Camera cam;
    private List<LineRenderer> lineRenderer;
    [SerializeField]
    private float minDistance;
    [SerializeField]
    private LayerMask layer;
    private bool isHeld;
    public InputSystem_Actions _inputAction { get; private set; }
    private Vector3 previousPosition;
    DrawSystem drawSystem;
    LineRenderer lr;
    void Start()
    {
        _inputAction = new InputSystem_Actions();
        _inputAction.Hieroglyphic.Paint.performed += ClickRay;
        _inputAction.Hieroglyphic.Paint.canceled += ClickRay;
        _inputAction.Hieroglyphic.Finish.performed += PaintingFinished;
        // _inputAction.Hieroglyphic.Paint.canceled += a; // crea dos quad por esto.
        lineRenderer = new List<LineRenderer>();
        // Set the material
        previousPosition = transform.position;
        lr = this.gameObject.AddComponent<LineRenderer>();
        lr.SetColors(Color.black, Color.black);
        lr.startWidth = 0.05f;                 // Ancho de la línea
        lr.endWidth = 0.05f;
        lineRenderer.Add(lr);
        lineRenderer[lineRenderer.Count - 1].material = new Material(Shader.Find("Sprites/Default"));

        // Set the number of vertices
        lineRenderer[lineRenderer.Count - 1].positionCount = 2;

        // Set the positions of the vertices
        // lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        // lineRenderer.SetPosition(1, new Vector3(1, 1, 0));
        // lineRenderer.SetPosition(2, new Vector3(2, 0, 0));
    }
  /*  public void a(InputAction.CallbackContext context)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 5, layer))
        {
            drawSystem.SetPoint(hit, cam, hit.point.z+0.1f);
        }
    }*/
    private void ClickRay(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isHeld = true;
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit, 5, layer))
            {
                Vector3 currentPosition = new Vector3(hit.point.x, hit.point.y, hit.point.z + 0.1f);
                if (Vector3.Distance(currentPosition, previousPosition) > minDistance)
                {
                    if (previousPosition == transform.position || previousPosition == hit.point)
                    {
                        lineRenderer[lineRenderer.Count - 1].SetPosition(0, currentPosition);
                    }
                    else
                    {
                        lineRenderer[lineRenderer.Count - 1].SetPosition(0, currentPosition);
                    }

                    previousPosition = currentPosition;
                }
            }
            StartCoroutine(Held());
        }
        else if (context.canceled)
        {
            isHeld = false;
            GameObject lineObject = new GameObject("Line");
            lineObject.transform.parent = this.transform;
            lineObject.layer = 23;
            // Añade un LineRenderer a ese nuevo;
            lr = lineObject.AddComponent<LineRenderer>();
            lr.SetColors(Color.black, Color.black);
            lr.startWidth = 0.05f;                 // Ancho de la línea
            lr.endWidth = 0.05f;
            lineRenderer.Add(lr);
            lineRenderer[lineRenderer.Count - 1].material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer[lineRenderer.Count - 1].positionCount = 2;
        }
    }
    IEnumerator Held()
    {
        //lineRenderer[lineRenderer.Count - 1].positionCount++;
        while (isHeld)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                          lineRenderer[lineRenderer.Count - 1].SetPosition(1, new Vector3(hit.point.x, hit.point.y, hit.point.z + 0.1f));
            }

            yield return new WaitForEndOfFrame();
        }

    }

    void PaintingFinished(InputAction.CallbackContext context)
    {
        ocr.CaptureDrawing();
        for(int i = 0; i <  this.transform.childCount; i++)
        {
            if(!this.transform.GetChild(i).gameObject.TryGetComponent<Camera>(out Camera cam))
                Destroy(this.transform.GetChild(i).gameObject);
        }
        this.GetComponent<LineRenderer>().positionCount = 0;
        this.GetComponent<LineRenderer>().positionCount = 2;
        lineRenderer.Clear();
        lineRenderer.Add(this.GetComponent<LineRenderer>());
    }
}
