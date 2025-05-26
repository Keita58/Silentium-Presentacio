using UnityEngine;
using UnityEngine.VFX;

public class ThrowParticles : MonoBehaviour
{
    //[SerializeField] Player player;

    private void Awake()
    {
        //player.OnThrow +=StartParticle;
    }

    public void StartParticle()
    {
        this.GetComponent<VisualEffect>().Play();
    }
}
