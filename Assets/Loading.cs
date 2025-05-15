using System.Collections;
using UnityEngine;

public class Loading : MonoBehaviour
{
    [SerializeField] Animator animator;
    private void Awake()
    {
        Debug.Log("Awaken");
        this.gameObject.SetActive(false);
        animator = GetComponent<Animator>();
        GameManager.instance.onLoadedScene += ActivateLoading;
    }

    private void ActivateLoading()
    {
        animator.Play("Loading");
        this.gameObject.SetActive(true);
        StartCoroutine(LoadingCoroutine());
    }

    private IEnumerator LoadingCoroutine()
    {
        yield return new WaitForSeconds(5f);
        this.gameObject.SetActive(false);
    }
}
