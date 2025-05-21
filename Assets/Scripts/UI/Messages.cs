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
        events.OnInventoryWarning += ShowWarningMessage;
    }

    private void ShowMessageInteractuable(GameObject interactableGameObject)
    {
        switch (interactableGameObject.layer)
        {
            case 13:
                messageText.text = "Pulsa E para abrir el cofre";
                break;
            case 12:
                messageText.text = "Pulsa E para coger la nota";
                break;
            case 9:
                string itemName = interactableGameObject.GetComponent<PickItem>().item.name;
                messageText.text = "Pulsa E para coger "+itemName;
                break;
            default:
                messageText.text = "Pulsa E para interactuar";
                break;
        }
    }

    private void HideMessage()
    {
        messageText.text = "";
    }

    private void ShowWarningMessage(string text)
    {
        messageText.text = text;
        StartCoroutine(DoAction(3f, () => HideMessage()));
    }

    IEnumerator DoAction(float tempsDespera, Action action)
    {
        if (tempsDespera > 0)
            yield return new WaitForSeconds(tempsDespera);
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
