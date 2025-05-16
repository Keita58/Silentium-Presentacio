using System.Collections;
using UnityEngine;

public class Loading : MonoBehaviour
{
    [SerializeField] Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
        GameManager.instance.onLoadedScene += ActivateLoading;
    }

    private void ActivateLoading()
    {
        animator.Play("Loading");
        StartCoroutine(LoadingCoroutine());
    }

    private IEnumerator LoadingCoroutine()
    {
        this.gameObject.SetActive(true);
        yield return new WaitForSeconds(5f);
        this.gameObject.SetActive(false);
    }
}
