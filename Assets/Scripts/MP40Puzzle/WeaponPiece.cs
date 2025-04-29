using UnityEngine;

public class WeaponPiece : MonoBehaviour
{
    [SerializeField]
    WeaponPiece DownLeft;
    [SerializeField]
    WeaponPiece TopRight;
    [SerializeField]
    private Collider colliderDownLeft;
    [SerializeField]
    private Collider colliderDownRight;
    private bool isClicked = false;
    private void OnMouseDown()
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

    }
}
