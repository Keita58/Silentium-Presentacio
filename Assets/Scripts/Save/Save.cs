using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static InventorySO;

public class Save : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";
    private const string _SavefileNameConfig = "silentium_config.json";

    [Header("Inventories")]
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;

    [Header("DataBase for the notes")]
    [SerializeField] private BDNotes _Notes;

    [Header("Imports")]
    [SerializeField] private Player _Player;
    [SerializeField] private PuzzleManager _PuzzleManager;

    [Header("Shaders")]
    [SerializeField] private GameObject _Shaders;

    public void SaveGame()
    {
        string filePath = Application.persistentDataPath + "/" + _SavefileName;

        //Canviem el tipus de la llista per poder guardar tota la info
        List<ItemSlotSave> Inventory = new List<ItemSlotSave>();

        foreach (var item in _Inventory.items)
        {
            Inventory.Add(new ItemSlotSave(item.item.id, item.amount, item.stackable));
        }

        List<ItemSlotSave> ChestInventory = new List<ItemSlotSave>();

        foreach (var item in _ChestInventory.items)
        {
            ChestInventory.Add(new ItemSlotSave(item.item.id, item.amount, item.stackable));
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
            Fog = Fog.active
        };

        string infoToSave = JsonUtility.ToJson(info, true);
        File.WriteAllText(filePath, infoToSave);
    }

    public void SaveConfigMenu()
    {
        string filePath = Application.persistentDataPath + "/" + _SavefileNameConfig;

        SaveConfig config = new()
        {
            //Info de la configuracio
        };

        string infoToSave = JsonUtility.ToJson(config, true);
        File.WriteAllText(filePath, infoToSave);
    }
}
