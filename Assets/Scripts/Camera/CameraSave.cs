using System;
using System.Collections;
using UnityEngine;

public class CameraSave : MonoBehaviour
{
    public event Action onSaveGame;

    [SerializeField] private GameObject _Player;
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private GameObject _SaveText;
    [SerializeField] private Events events;

    private bool _SavedGame;

    private void Awake()
    {
        _SavedGame = false;
        GetComponent<InteractuableCamera>().OnCameraClick += Save;
    }

    public void Save()
    {
        foreach (var item in _Inventory.items) 
        {
            if(item.item.GetType() == typeof(SaveItem))
            {
               _Inventory.UseItem(item.item);

                _SavedGame = true;
                onSaveGame?.Invoke();
                break;
            }
        }

        if (_SavedGame)
        {
            _SaveText.SetActive(true);
            StartCoroutine(DeleteText());
            _SavedGame = false;
        }
        else
        {
            events.ShowWarning("No tienes el objeto necesario para guardar!");
        }
    }

    IEnumerator DeleteText()
    {
        yield return new WaitForSeconds(5);
        _SaveText.SetActive(false);
    }
}
