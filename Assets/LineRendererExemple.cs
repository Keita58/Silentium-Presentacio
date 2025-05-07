// Programatically add a LineRenderer component and draw a 3D line.
using UnityEngine;
using UnityEngine.InputSystem;

public class LineRendererExample : MonoBehaviour
{

    [SerializeField]
    Camera cam;
    [SerializeField]
    LayerMask layer;
    [SerializeField]
    private LineRenderer lineRenderer;
    [SerializeField]
    public InputSystem_Actions _inputAction { get; private set; }
    private int index = 0;
    void Start()
    {
        _inputAction = new InputSystem_Actions();
        _inputAction.Hieroglyphic.Paint.performed += ClickRay;
        _inputAction.Hieroglyphic.Paint.canceled += ClickRay;
        // Set the material
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));

        // Set the color
        lineRenderer.startColor = Color.red;
        lineRenderer.endColor = Color.green;

        // Set the width
        lineRenderer.startWidth = 0.2f;
        lineRenderer.endWidth = 0.2f;

        // Set the number of vertices
        lineRenderer.positionCount = 20;

        // Set the positions of the vertices
        lineRenderer.SetPosition(0, new Vector3(0, 0, 0));
        lineRenderer.SetPosition(1, new Vector3(1, 1, 0));
        lineRenderer.SetPosition(2, new Vector3(2, 0, 0));
    }
     private void ClickRay(InputAction.CallbackContext context)
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 1;
        Debug.Log("Mouse Position: " + cam.ScreenToWorldPoint(mousePos));
        Ray ray = cam.ScreenPointToRay(mousePos);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layer))
        {
            lineRenderer.SetPosition(index, hit.point);
            index++;
            if (index >= lineRenderer.positionCount)
            {
                index = 0;
            }
        }
    }
}