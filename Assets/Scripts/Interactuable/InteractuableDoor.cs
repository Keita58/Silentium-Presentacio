using UnityEngine;

[RequireComponent(typeof(Door))]
public class InteractuableDoor : MonoBehaviour, IInteractuable
{
    public bool isRemarkable { get; private set; }
    public bool isInteractuable { get;  set; }
    Door door;
    [SerializeField] private Events events;

    [Header("Audio")]
    [SerializeField]
    AudioClip audioOpenPlayer;
    [SerializeField]
    AudioClip audioClosePlayer;
    [SerializeField]
    AudioClip audioOpenEnemy;
    [SerializeField]
    AudioClip audioCloseEnemy;    
    [SerializeField]
    AudioClip audioDoorJammed;
    private AudioSource doorAudioSource;
    private void Awake()
    {
        isRemarkable = false;
        isInteractuable = true;
        door = GetComponent<Door>();
        doorAudioSource = GetComponent<AudioSource>();
    }

    public void Interact()
    {
    }
    public void SoundPlayer()
    {
        if (!door.isLocked)
        {
            if (this.door.isOpen)
                doorAudioSource.PlayOneShot(audioClosePlayer);
            else
                doorAudioSource.PlayOneShot(audioOpenPlayer);
        }
        else
        {
            doorAudioSource.PlayOneShot(audioDoorJammed);
        }
    }
    public void SoundEnemy()
    {
        if (this.door.isOpen)
            doorAudioSource.PlayOneShot(audioCloseEnemy);
        else
            doorAudioSource.PlayOneShot(audioOpenEnemy);
    }

    public void Interact(Transform player)
    {
        if (door.isLocked)
        {
            InventorySO.ItemSlot aux = null;
            foreach (InventorySO.ItemSlot item in InventoryManager.instance.inventory.items)
            {
                if (item.item == door.itemNeededToOpen)
                {
                    door.isLocked = false;
                    aux = item;
                    InventoryManager.instance.inventory.UseItem(aux.item);
                    break;
                }
            }
            if (door.isOpen)
            {
                door.Close();
            }
            else
            {
                door.Open(player.position);
            }

            if (aux == null)
            {
                if (door.itemNeededToOpen == null)
                    events.ShowWarning("No puedo abrirla.");
                else
                    events.ShowWarning("Creo que se necesita " + door.itemNeededToOpen.name + " para abrir esta puerta, pero no recuerdo bien.");
            }
        }
        else
        {
            if (door.isOpen)
            {
                door.Close();
            }
            else
            {
                door.Open(player.position);
            }
        }
    }
}
