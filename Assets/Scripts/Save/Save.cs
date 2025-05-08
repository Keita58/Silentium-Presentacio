using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static InventorySO;

public class Save : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";

    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;
    [SerializeField] private BDNotes _Notes;
    [SerializeField] private Player _Player;
    [SerializeField] private PuzzleManager _PuzzleManager;

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
            WeaponPuzzle = _PuzzleManager.weaponPuzzleCompleted
        };

        string infoToSave = JsonUtility.ToJson(info, true);
        File.WriteAllText(filePath, infoToSave);
    }
}
