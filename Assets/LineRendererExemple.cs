// Programatically add a LineRenderer component and draw a 3D line.

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LineRendererExample : MonoBehaviour
{

    [SerializeField]
    Camera cam;
    private LineRenderer lineRenderer;
    [SerializeField]
    private float minDistance;
    [SerializeField]
    private LayerMask layer;
    private bool isHeld;
    public InputSystem_Actions _inputAction { get; private set; }
    private Vector3 previousPosition;
    DrawSystem drawSystem;
    void Start()
    {
        drawSystem = new DrawSystem();
        _inputAction = new InputSystem_Actions();
        _inputAction.Hieroglyphic.Paint.performed += a;
        _inputAction.Hieroglyphic.Paint.canceled += a; // crea dos quad por esto.
        // Set the material
        previousPosition = transform.position;
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Set the number of vertices
        lineRenderer.positionCount = 1;

        // Set the positions of the vertices
        // lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        // lineRenderer.SetPosition(1, new Vector3(1, 1, 0));
        // lineRenderer.SetPosition(2, new Vector3(2, 0, 0));
    }
    public void a(InputAction.CallbackContext context)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 5, layer))
        {
            drawSystem.SetPoint(hit, hit.point.z+0.1f);
        }
    }
    /*private void ClickRay(InputAction.CallbackContext context)
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
                    if (previousPosition == transform.position)
                    {
                        lineRenderer.SetPosition(0, currentPosition);
                    }
                    else
                    {
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(lineRenderer.positionCount - 1, currentPosition);
                    }

                    previousPosition = currentPosition;
                }
            }
            StartCoroutine(Held());
        }
        else if (context.canceled)
        {
            isHeld = false;
        }
    }
    IEnumerator Held()
    {
        lineRenderer.positionCount++;
        while (isHeld)
        {
            Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                          lineRenderer.SetPosition(lineRenderer.positionCount - 1, new Vector3(hit.point.x, hit.point.y, hit.point.z + 0.1f));
            }

            yield return new WaitForEndOfFrame();
        }

    }*/
}
