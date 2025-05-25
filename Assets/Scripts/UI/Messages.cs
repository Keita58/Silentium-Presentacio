using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Messages : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private TextMeshProUGUI messageText;
    [SerializeField] private Events events;

    private void Awake()
    {
        player.OnInteractuable += ShowMessageInteractuable;
        player.OnNotInteractuable += HideMessage;
        events.OnWarning += ShowWarningMessage;
    }

    private void ShowMessageInteractuable(GameObject interactableGameObject)
    {
        IInteractuable interactuable = interactableGameObject.GetComponent<IInteractuable>();
        if (interactuable != null && interactuable is not InteractuableDoor)
        {
            if (interactuable is InteractuableChest) messageText.text = "Pulsa E para abrir el cofre";
            else if (interactuable is InteractuableNote) messageText.text = "Pulsa E para coger la nota";
            else if (interactuable is InteractuableItem)
            {
                string itemName = interactableGameObject.GetComponent<PickItem>().item.name;
                messageText.text = "Pulsa E para coger " + itemName;
            }else
                messageText.text = "Pulsa E para interactuar";
        }
    }

    private void HideMessage()
    {
        messageText.text = "";
    }

    private void HideMessageWarning()
    {
        messageText.text = "";
        player.OnInteractuable += ShowMessageInteractuable;
        player.OnNotInteractuable += HideMessage;
    }

    private void ShowWarningMessage(string text)
    {
        player.OnInteractuable -= ShowMessageInteractuable;
        player.OnNotInteractuable -= HideMessage;
        messageText.text = text;    
        StartCoroutine(DoAction(3f, () => HideMessageWarning()));
    }

    IEnumerator DoAction(float waitTime, Action action)
    {
        if (waitTime > 0)
            yield return new WaitForSeconds(waitTime);
        else
            yield return null;
        action();
    }

    private void OnDestroy()
    {
        player.OnInteractuable -= ShowMessageInteractuable;
        player.OnNotInteractuable -= HideMessage;
    }

}
