using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class CameraSave : MonoBehaviour
{
    public event Action onSaveGame;

    [SerializeField] private GameObject _Player;
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private TextMeshProUGUI _SaveText;

    private bool _SavedGame;

    private void Start()
    {
        _SavedGame = false;
        _Player.GetComponent<Player>().onCameraClick += Save;
    }

    public void Save()
    {
        foreach (var item in _Inventory.items) 
        {
            if(item.GetType() == typeof(SaveItem))
            {
                if (item.amount >= 2)
                    item.amount--;
                else
                    _Inventory.items.Remove(item);

                _SavedGame = true;
                onSaveGame?.Invoke();
            }
        }

        if (_SavedGame)
        {
            _SaveText.enabled = true;
            StartCoroutine(DeleteText());
            _SavedGame = false;
        }
    }

    IEnumerator DeleteText()
    {
        yield return new WaitForSeconds(2);
        _SaveText.enabled = false;
    }
}
