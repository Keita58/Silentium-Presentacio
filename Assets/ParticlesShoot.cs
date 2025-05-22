using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class ParticlesShoot : MonoBehaviour
{
    [SerializeField] Player player;
    [SerializeField] Light lightShoot;

    private void Awake()
    {
        player.OnShoot += PlayParticles;
    }

    public void PlayParticles()
    {
        GetComponent<VisualEffect>().Play();
        lightShoot.enabled = true;
        StartCoroutine(StopLight());

    }

    public IEnumerator StopLight()
    {
        yield return new WaitForSeconds(0.1f);
        lightShoot.enabled = false;
    }
}
