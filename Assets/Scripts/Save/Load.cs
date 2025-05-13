using System.IO;
using UnityEngine;
using System.Collections.Generic;
using static InventorySO;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class Load : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";
    private const string _SavefileNameConfig = "silentium_config.json";

    [Header("Inventories")]
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;

    [Header("DataBase for the notes")]
    [SerializeField] private BDNotes _Notes;
    [SerializeField] private BDItems _Items;

    [Header("Shaders")]
    [SerializeField] private GameObject _Shaders;

    [Header("Player")]
    [SerializeField] private Player _Player;

    [Header("Puzzle manager")]
    [SerializeField] private PuzzleManager _PuzzleManager;

    [Header("Settings")]
    [SerializeField] private Settings _Settings;

    public void LoadGame()
    {
        string existingFile = Application.persistentDataPath + "/" + _SavefileName;

        if(File.Exists(existingFile))
        {
            string jsonContent = File.ReadAllText(existingFile);
            SaveInfo info = JsonUtility.FromJson<SaveInfo>(jsonContent);

            List<ItemSlot> Inventory = new List<ItemSlot>();

            foreach (var item in info.Inventory)
            {
                Inventory.Add(new ItemSlot(_Items.FromID(item.itemId), item.amount, item.stackable));
            }

            List<ItemSlot> ChestInventory = new List<ItemSlot>();

            foreach (var item in info.ChestInventory)
            {
                ChestInventory.Add(new ItemSlot(_Items.FromID(item.itemId), item.amount, item.stackable));
            }

            _Inventory.items = Inventory;
            _ChestInventory.items = ChestInventory;
            _NotesInventory.notes = _Notes.FromIDs(info.NotesInventory);

            //Treure la info del Volume
            Volume VolumeInfo = _Shaders.GetComponent<Volume>();

            VolumeInfo.profile.TryGet(out ChromaticAberration Chromatic);
            Chromatic.active = info.ChromaticAberration;
            Chromatic.intensity.value = info.IntensityChromaticAberration;
            Chromatic.maxSamples = info.Samples;

            VolumeInfo.profile.TryGet(out FilmGrain Film);
            Film.active = info.FilmGrain;
            Film.intensity.value = info.IntensityFilmGrain;
            
            VolumeInfo.profile.TryGet(out Fog Fog);
            Fog.active = info.Fog;

            //Info del jugador
            _Player.hp = info.Hp;
            _Player.isSilencerEquipped = info.Silencer;
            _Player.silencerUses = info.SilencerUses;
            _Player.gunAmmo = info.Ammo;

            //Puzles
            _PuzzleManager.clockPuzzleCompleted = info.ClockPuzzle;
            _PuzzleManager.isHieroglyphicCompleted = info.HieroglyphicPuzzle;
            _PuzzleManager.bookPuzzleCompleted = info .BookPuzzle;
            _PuzzleManager.poemPuzzleCompleted = info.PoemPuzzle;
            _PuzzleManager.isMorseCompleted = info .MorsePuzzle;
            _PuzzleManager.weaponPuzzleCompleted = info.WeaponPuzzle;

            _PuzzleManager.LoadGame();
        }
    }

    public void LoadConfig()
    {
        string existingFile = Application.persistentDataPath + "/" + _SavefileNameConfig;

        if (File.Exists(existingFile))
        {
            string jsonContent = File.ReadAllText(existingFile);
            SaveConfig info = JsonUtility.FromJson<SaveConfig>(jsonContent);

            _Settings.musicSlider.value = info.MusicValue;
            _Settings.sfxSlider.value = info.SFXValue;
            _Settings.fpsDropdown.value = info.FPSValue;
            _Settings.fovSlider.value = info.FOVValue;
            _Settings.vSyncToggle.enabled = info.VSync;
        }

        //Posar booleà a true i actualitzar amb el load
    }
}
