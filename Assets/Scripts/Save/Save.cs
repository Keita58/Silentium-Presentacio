using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static InventorySO;
using static PickObject;
using static CellBook;

public class Save : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";
    private const string _TemporalSavefileName = "silentium_temp_savegame.json";
    private const string _SavefileNameConfig = "silentium_config.json";

    private string filepath;

    [Header("Cameras")]
    [SerializeField] private CameraSave _Camera1;
    [SerializeField] private CameraSave _Camera2;
    
    [Header("Inventories")]
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;
    [SerializeField] private InventoryManager _InventoryManager;

    [Header("DataBase for the notes")]
    [SerializeField] private BDNotes _Notes;

    [Header("Imports")]
    [SerializeField] private Player _Player;
    [SerializeField] private PuzzleManager _PuzzleManager;

    [Header("Shaders")]
    [SerializeField] private GameObject _Shaders;

    [Header("Settings")]
    [SerializeField] private Settings _Settings;

    [Header("Spawn items")]
    [SerializeField] private GameObject[] ImportantSpawns;
    [SerializeField] private GameObject[] NormalSpawns;

    [Header("Monsters")]
    [SerializeField] private Transform _Blind;
    [SerializeField] private Transform _Fast;
    [SerializeField] private Transform _Fat;

    [Header("Books puzzle")]
    [SerializeField] private List<CellBook> _Cells;

    private bool _Delete;
    private bool _Temp;

    private void Awake()
    {
        if(_Camera1 != null && _Camera2 != null)
        {
            _Camera1.onSaveGame += SaveGame;
            _Camera2.onSaveGame += SaveGame;
        }
        filepath = "";
        _Delete = false;
        _Temp = false;
    }

    private void Start()
    {
        _Settings.onSaveConfig += SaveConfigMenu;
    }

    public void SaveGame()
    {
        if(filepath == "")
            filepath = Application.persistentDataPath + "/" + _SavefileName;

        //Canviem el tipus de la llista per poder guardar tota la info
        List<ItemSlotSave> Inventory = new List<ItemSlotSave>();

        foreach (ItemSlot item in _Inventory.items)
        {
            if(!_Temp && (item.item is ThrowableItem || item.item is SilencerItem || item.item is HealingItem || item.item is AmmunitionItem) && !_Delete)
            {
                _Delete = true;
            }
            else
                Inventory.Add(new ItemSlotSave(item.item.id, item.amount, item.stackable));
        }

        List<ItemSlotSave> ChestInventory = new List<ItemSlotSave>();

        foreach (ItemSlot item in _ChestInventory.items)
        {
            if (!_Temp && (item.item is ThrowableItem || item.item is SilencerItem || item.item is HealingItem || item.item is AmmunitionItem) && !_Delete)
            {
                _Delete = true;
            }
            else
                ChestInventory.Add(new ItemSlotSave(item.item.id, item.amount, item.stackable));
        }

        List<PickObjectSave> ImportantSpawnsList = new List<PickObjectSave>();

        foreach(GameObject item in ImportantSpawns)
        {
            ImportantSpawnsList.Add(new PickObjectSave(item.GetComponent<PickObject>().Object.id, item.GetComponent<PickObject>().Id, item.GetComponent<PickObject>().Picked));
        }

        List<PickObjectSave> NormalSpawnsList = new List<PickObjectSave>();

        foreach (GameObject item in NormalSpawns)
        {
            NormalSpawnsList.Add(new PickObjectSave(item.GetComponent<PickObject>().Object.id, item.GetComponent<PickObject>().Id, item.GetComponent<PickObject>().Picked));
        }

        List<CellBookSave> CellBooks = new List<CellBookSave>();

        foreach(CellBook book in _Cells)
        {
            if (book.bookGO != null)
                CellBooks.Add(new CellBookSave(book.bookGO.GetComponent<PickItem>().item.id, book.cellId));
        }

        //Treure la info del Volume
        Volume VolumeInfo = _Shaders.GetComponent<Volume>();
        VolumeInfo.profile.TryGet(out ChromaticAberration Chromatic);
        VolumeInfo.profile.TryGet(out FilmGrain Film);
        VolumeInfo.profile.TryGet(out Fog Fog);

        SaveInfo info = new()
        {
            Inventory = Inventory,
            ChestInventory = ChestInventory,
            NotesInventory = _Notes.ToIDs(_NotesInventory.notes.AsReadOnly()),
            Hp = _Player.hp,
            Silencer = _Player.isSilencerEquipped,
            SilencerUses = _Player.silencerUses,
            Ammo = _Player.gunAmmo,
            Position = _Player.transform.position,
            EquipedItem = _InventoryManager.equippedItem != null ? InventoryManager.instance.equippedItem.id : -1,
            ClockPuzzle = _PuzzleManager.clockPuzzleCompleted,
            HieroglyphicPuzzle = _PuzzleManager.isHieroglyphicCompleted,
            BookPuzzle = _PuzzleManager.bookPuzzleCompleted,
            PoemPuzzle = _PuzzleManager.poemPuzzleCompleted,
            MorsePuzzle = _PuzzleManager.isMorseCompleted,
            WeaponPuzzle = _PuzzleManager.weaponPuzzleCompleted,
            ChromaticAberration = Chromatic.active,
            IntensityChromaticAberration = Chromatic.intensity.value,
            Samples = Chromatic.maxSamples,
            FilmGrain = Film.active,
            IntensityFilmGrain = Film.intensity.value,
            Fog = Fog.active,
            FastEnemy = _Fast.position,
            FatEnemy = _Fat.position,
            BlindEnemy = _Blind.position,
            ImportantSpawns = ImportantSpawnsList,
            NormalSpawns = NormalSpawnsList,
            CellBooks = CellBooks,
        };

        string infoToSave = JsonUtility.ToJson(info, true);
        File.WriteAllText(filepath, infoToSave);
    }

    public void TemporalSaveGame()
    {
        filepath = Application.persistentDataPath + "/" + _TemporalSavefileName;
        _Temp = true;
        SaveGame();
        Application.Quit();
    }

    public void SaveConfigMenu()
    {
        string filePath = Application.persistentDataPath + "/" + _SavefileNameConfig;

        SaveConfig config = new()
        {
            MusicValue = _Settings.currentVolumeValue,
            SFXValue = _Settings.currentSfxValue,
            FPSValue = _Settings.currentFpsValue,
            FOVValue = _Settings.currentFOVValue,
            VSync = _Settings.currentVSyncState
        };

        string infoToSave = JsonUtility.ToJson(config, true);
        File.WriteAllText(filePath, infoToSave);
    }
}
