using System;
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

    private void OnDestroy()
    {
        GameManager.instance.onLoadedScene -= ActivateLoading;
    }

    private void ActivateLoading()
    {
        animator.Play("Loading");
        gameObject.SetActive(true);
        StartCoroutine(LoadingCoroutine());
    }

    private IEnumerator LoadingCoroutine()
    {
        yield return new WaitForSeconds(5f);
        gameObject.SetActive(false);
    }
}
