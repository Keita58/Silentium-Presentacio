using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponPuzzle : MonoBehaviour
{
    [SerializeField]
    WeaponPiece DownLeft;
    [SerializeField]
    WeaponPiece TopRight;
    [SerializeField]
    WeaponPiece TopLeft;
    [SerializeField]
    private Camera cam;
    [SerializeField]
    private GameObject reward;
    public InputSystem_Actions inputAction;

    private void Awake()
    {
        inputAction = new InputSystem_Actions();
        inputAction.WeaponPuzzle.Unmount.performed += ClickRay;
        inputAction.WeaponPuzzle.Unmount.canceled += ClickRay;

    }

    /*  private void OnMouseDown()
      {
          if (gameObject.tag == "FrontTop")
          {
              this.gameObject.GetComponent<Animator>().Play("unmountFrontTopWeapon");
              isClicked = true;
              colliderDownLeft.enabled = true;
          }
          else if (gameObject.tag == "FrontDown")
          {
              this.gameObject.GetComponent<Animator>().Play("unmountFrontDownWeapon");
              if (TopRight.isClicked)
                  colliderDownRight.enabled = true;
          }
          else if (gameObject.tag == "WeaponTopRight")
          {
              this.gameObject.GetComponent<Animator>().Play("unmountTopRightWeapon");
              if(DownLeft.isClicked)
                  colliderDownRight.enabled = true;
          }
          else if (gameObject.tag == "WeaponDownRight")
          {
              this.gameObject.GetComponent<Animator>().Play("unmountDownRightWeapon");

          }

      }*/

    private void ClickRay(InputAction.CallbackContext context)
    {
        Vector3 mousePos = Mouse.current.position.ReadValue();
        mousePos.z = 1;
        Debug.Log("Mouse Position: " + cam.ScreenToWorldPoint(mousePos));
        Ray ray = cam.ScreenPointToRay(mousePos);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.yellow);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("FrontTop"))
                {
                    hit.transform.GetComponent<Animator>().Play("unmountFrontTopWeapon");
                    hit.transform.GetComponent<WeaponPiece>().isClicked = true;
                }
                else if (hit.collider.CompareTag("FrontDown"))
                {
                    Debug.Log("FrontDown clicked"+" This is the top: "+TopLeft.isClicked);
                    if (TopLeft.isClicked)
                    {
                        hit.transform.GetComponent<Animator>().Play("FrontDownWeapon");
                        hit.transform.GetComponent<WeaponPiece>().isClicked = true;
                    }
                }
                else if (hit.collider.CompareTag("WeaponTopRight"))
                {
                    hit.transform.GetComponent<Animator>().Play("unmountTopRightWeapon");
                    hit.transform.GetComponent<WeaponPiece>().isClicked = true;
                }
                else if (hit.collider.CompareTag("WeaponDownRight"))
                {
                    if(TopLeft.isClicked && TopRight.isClicked && DownLeft.isClicked) { 
                        hit.transform.GetComponent<Animator>().Play("unmountDownRightWeapon");
                        reward.SetActive(true);
                        PuzzleManager.instance.ExitWeaponPuzzle();
                    }
                }
            }
            else
            {
                Debug.Log("No hit");
            }
        }

    }
}
