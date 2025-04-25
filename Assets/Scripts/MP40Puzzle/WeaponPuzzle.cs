using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPuzzle : MonoBehaviour
{
    public InputSystem_Actions inputActions { get; private set; }
    [SerializeField]
    List<GameObject> unmountedParts;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        unmountedParts = new List<GameObject>();
        inputActions = new InputSystem_Actions();
        inputActions.WeaponPuzzle.Unmount.performed += Unmount;
        inputActions.WeaponPuzzle.Unmount.canceled += Unmount;
    }

    void Unmount(InputAction.CallbackContext context)
    {
            RaycastHit raycastHit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out raycastHit, 100f))
            {
                if (raycastHit.transform != null)
                {
                    //Our custom method. 
                    CurrentClickedGameObject(raycastHit.transform.gameObject);
                }
            }
    }
    public void CurrentClickedGameObject(GameObject gameObject)
    {
        unmountedParts.Add(gameObject);
        if (gameObject.tag == "FrontTop")
        {

        }else if (gameObject.tag == "FrontDown")
        {

        }else if (gameObject.tag == "WeaponMiddle")
        {

        }else if (gameObject.tag == "WeaponTopRight")
        {

        }else if (gameObject.tag == "WeaponDownRight")
        {

        }
    }

}
