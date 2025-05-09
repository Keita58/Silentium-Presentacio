using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.ProBuilder.MeshOperations;

public class DrawSystem : MonoBehaviour
{
    //[SerializeField]
    GameObject quad;
    private bool held;
    public DrawSystem()
    {
    }
    public void SetPoint(RaycastHit hit, float z = Mathf.Infinity)
    {
        if (z == Mathf.Infinity)
        {
            z = hit.point.z;
        }
        quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
        quad.GetComponent<MeshRenderer>().materials[0] = new Material(Shader.Find("Sprites/Default"));
        quad.transform.position = new Vector3(hit.point.x, hit.point.y, z);
        quad.transform.eulerAngles = hit.transform.eulerAngles;
    }
}
