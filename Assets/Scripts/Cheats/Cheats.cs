using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

public class Cheats : MonoBehaviour
{
    public InputSystem_Actions inputActions { get; private set; }

    [Header("Player")] 
    [SerializeField] private GameObject _player;
    [SerializeField] private GameObject _cameraPlayer;
    private Player _playerComponent;

    [Header("Enemies cams")] 
    [SerializeField] private GameObject _blindCam;
    [SerializeField] private GameObject _bigCam;
    [SerializeField] private GameObject _fastCam;
    private bool _cameraBlindBool;
    private bool _cameraBigBool;
    private bool _cameraFastBool;

    [Header("Books")] 
    [SerializeField] List<BookItem> _books;
    [SerializeField] private List<GameObject> _booksPrefabs;
    [SerializeField] private bool bookCheatDone;

    [Header("Morse")] 
    [SerializeField] private bool morseCheatDone;
    
    [Header("Hieroglyphics")]
    [SerializeField] private bool hieroglyphicsCheatDone;
    
    [Header("Infinite lives")]
    private bool _infiniteLives;

    [Header("Directional light")] 
    [SerializeField] private GameObject _light;
    
    [Header("Chest")]
    [SerializeField] private List<InventorySO.ItemSlot> _chest;
    
    [Header("Notes")]
    [SerializeField] private List<NotesSO> _notes;
    [SerializeField] private List<GameObject> _notesPrefabs;

    private void Awake()
    {
        _cameraBlindBool = false;
        _cameraBigBool = false;
        _cameraFastBool = false;
        _infiniteLives = false;
        bookCheatDone = false;
        
        _playerComponent = _player.GetComponent<Player>();

        inputActions = new InputSystem_Actions();
        inputActions.Cheats.Jeroglifics.started += EnableCheatsHieroglyphics;
        inputActions.Cheats.CameraCec.started += CameraBlindEnemy;
        inputActions.Cheats.CameraGras.started += CameraBigEnemy;
        inputActions.Cheats.CameraRapid.started += CameraFastEnemy;
        inputActions.Cheats.CodiMorse.started += EnableCheatsMorse;
        inputActions.Cheats.LlibresPuzle.started += EnablCheatsBook;
        inputActions.Cheats.VidaInfinita.started += InfiniteLifes;
        inputActions.Cheats.OmplirCofre.started += FullChest;
        inputActions.Cheats.NotesDiari.started += PostProcess;

        inputActions.Cheats.Enable();
    }

    private void PostProcess(InputAction.CallbackContext context)
    {
        foreach (NotesSO nota in _notes)
        {
            InventoryManager.instance.DiscoverNote(nota);
        }
        
        foreach (GameObject nota in _notesPrefabs)
        {
            nota.SetActive(false);
        }
    }

    private void FullChest(InputAction.CallbackContext context)
    {
        InventoryManager.instance.chestInventory.items = _chest;
    }

    private void InfiniteLifes(InputAction.CallbackContext context)
    {
        if(!_infiniteLives)
            _player.GetComponent<Player>().hp = int.MaxValue;
        else
            _player.GetComponent<Player>().hp = _player.GetComponent<Player>().maxHp;
    }

    private void EnablCheatsBook(InputAction.CallbackContext context)
    {
        if (!bookCheatDone)
        {
            InventoryManager.instance.inventory.items.Clear();
            foreach (BookItem book in _books)
            {
                InventorySO.ItemSlot aux = InventoryManager.instance.inventory.GetItem(book);
                if(!InventoryManager.instance.inventory.items.Contains(aux))
                    InventoryManager.instance.AddItem(book);
            }

            foreach (GameObject bookPrefab in _booksPrefabs)
            {
                bookPrefab.SetActive(false);
            }

            bookCheatDone = true;
        }
    }

    private void EnableCheatsMorse(InputAction.CallbackContext obj)
    {
        if (!morseCheatDone)
        {
            PuzzleManager.instance.ExitMorsePuzzleAnimation();
            morseCheatDone = true;
        }
    }

    private void EnableCheatsHieroglyphics(InputAction.CallbackContext obj)
    {
        if (!hieroglyphicsCheatDone)
        {
            PuzzleManager.instance.HieroglyphicPuzzleExitAnimation();
            hieroglyphicsCheatDone = true;
        }
    }

    private void CameraBlindEnemy(InputAction.CallbackContext obj)
    {
        if (!_cameraBlindBool)
        {
            _cameraPlayer.SetActive(false);
            _blindCam.SetActive(true);
            _bigCam.SetActive(false);
            _fastCam.SetActive(false);
            _cameraBlindBool = true;
            _light.SetActive(true);
        }
        else
        {
            _cameraPlayer.SetActive(true);
            _playerComponent.ResumeInteract(true);
            _blindCam.SetActive(false);
            _cameraBlindBool = false;
            _light.SetActive(false);
        }
    }

    private void CameraBigEnemy(InputAction.CallbackContext obj)
    {
        if (!_cameraBigBool)
        {
            _cameraPlayer.SetActive(false);
            _blindCam.SetActive(false);
            _bigCam.SetActive(true);
            _fastCam.SetActive(false);
            _cameraBigBool = true;
            _light.SetActive(true);
        }
        else
        {
            _cameraPlayer.SetActive(true);
            _playerComponent.ResumeInteract(true);
            _bigCam.SetActive(false);
            _cameraBigBool = false;
            _light.SetActive(false);
        }
    }

    private void CameraFastEnemy(InputAction.CallbackContext obj)
    {
        if (!_cameraFastBool)
        {
            _cameraPlayer.SetActive(false);
            _blindCam.SetActive(false);
            _bigCam.SetActive(false);
            _fastCam.SetActive(true);
            _cameraFastBool = true;
            _light.SetActive(true);
        }
        else
        {
            _cameraPlayer.SetActive(true);
            _playerComponent.ResumeInteract(true);
            _fastCam.SetActive(false);
            _cameraFastBool = false;
            _light.SetActive(false);
        }
    }
}