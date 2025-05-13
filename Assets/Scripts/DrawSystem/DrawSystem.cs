using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.ProBuilder.MeshOperations;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI.Table;

public class DrawSystem : MonoBehaviour
{
    //[SerializeField]
    private GameObject quad;
    private bool firstVertex;
    private float CamZDistance;
    Vector3 firstVertPos;
    public DrawSystem()
    {
        firstVertex = true;
    }
    public void SetPoint(RaycastHit hit, Camera cam, float z = Mathf.Infinity)
    {
        if (firstVertex)
        {
            if (z == Mathf.Infinity)
            {
                z = hit.point.z;
            }
            quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Sprites/Default"));
            quad.transform.position = new Vector3(hit.point.x, hit.point.y, z);
            quad.transform.rotation = Quaternion.Euler(new Vector3(quad.transform.rotation.x, 180, quad.transform.rotation.z));
            firstVertPos = hit.point;
            CamZDistance = cam.ScreenToWorldPoint(quad.transform.position).z;
            firstVertex = false;
        }
        else
        {
            Vector3 WorldMousePosition = cam.ScreenToWorldPoint(new Vector3(Mouse.current.position.ReadValue().x, Mouse.current.position.ReadValue().y, CamZDistance));
            //float dist = Vector3.Distance(hit.point, WorldMousePosition);
            float dist = Vector3.Distance(firstVertPos, WorldMousePosition);
            //quad.transform.localScale = new Vector3(Mathf.MoveTowards(cam.ScreenToWorldPoint(hit.point).x, quad.transform.localScale.y, quad.transform.localScale.z), quad.transform.localScale.y, quad.transform.localScale.z);
            quad.transform.localScale = new Vector3(dist / 2f,quad.transform.localScale.y,quad.transform.localScale.z);
            Vector3 mid = (firstVertPos + WorldMousePosition) / 2f;
            quad.transform.position = mid;
            Vector3 rot = WorldMousePosition - firstVertPos;
           // quad.transform.right = new Vector3(quad.transform.right.x,-rot.y,quad.transform.right.z);
            quad.transform.right = rot;
            //quad.transform.rotation.SetLookRotation(hit.point);
            firstVertex = true;
        }
    }
}
