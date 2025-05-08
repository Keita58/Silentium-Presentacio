// Programatically add a LineRenderer component and draw a 3D line.

using UnityEngine;
using UnityEngine.InputSystem;

public class LineRendererExample : MonoBehaviour
{

    [SerializeField]
    Camera cam;
    private LineRenderer lineRenderer;
    [SerializeField]
    private float minDistance;
    public InputSystem_Actions _inputAction { get; private set; }
    private Vector3 previousPosition;
    void Start()
    {

        _inputAction = new InputSystem_Actions();
        _inputAction.Hieroglyphic.Paint.performed += ClickRay;
        _inputAction.Hieroglyphic.Paint.canceled += ClickRay;
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
    private void ClickRay(InputAction.CallbackContext context)
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

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
}
