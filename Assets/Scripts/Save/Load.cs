using System.IO;
using UnityEngine;
using System.Collections.Generic;
using static InventorySO;
using static PickObject;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using static UnityEditor.Progress;
using Unity.VisualScripting;

public class Load : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";
    private const string _TemporalSavefileName = "silentium_temp_savegame.json";
    private const string _SavefileNameConfig = "silentium_config.json";

    [Header("Inventories")]
    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;

    [Header("DataBase for the notes")]
    [SerializeField] private BDNotes _Notes;
    [SerializeField] private BDItems _Items;
    [SerializeField] private BDBooks _Books;

    [Header("Shaders")]
    [SerializeField] private GameObject _Shaders;

    [Header("Player")]
    [SerializeField] private Player _Player;

    [Header("Puzzle manager")]
    [SerializeField] private PuzzleManager _PuzzleManager;

    [Header("Settings")]
    [SerializeField] private Settings _Settings;

    [Header("Monsters")]
    [SerializeField] private Transform _Blind;
    [SerializeField] private Transform _Fast;
    [SerializeField] private Transform _Fat;

    [Header("Spawn items")]
    [SerializeField] private List<PickObject> ImportantSpawns;
    [SerializeField] private List<PickObject> NormalSpawns;

    [Header("Books puzzle")]
    [SerializeField] private CellBook bookCell;
    [SerializeField] private CellBook bookCell2;
    [SerializeField] private CellBook bookCell3;
    [SerializeField] private CellBook bookCell4;
    [SerializeField] private CellBook bookCell5;
    [SerializeField] private CellBook bookCell6;
    [SerializeField] private CellBook bookCell7;
    [SerializeField] private CellBook bookCell8;
    [SerializeField] private CellBook bookCell9;
    [SerializeField] private CellBook bookCell10;
    [SerializeField] private CellBook bookCell11;
    [SerializeField] private CellBook bookCell12;
    [SerializeField] private CellBook bookCell13;
    [SerializeField] private CellBook bookCell14;
    [SerializeField] private CellBook bookCell15;
    [SerializeField] private CellBook bookCell16;


    private void Awake()
    {
        GameManager.instance.onLoadedScene += LoadGame;
        GameManager.instance.onLoadedScene += LoadConfig;

        GameManager.instance.onNewScene += LoadConfig;
    }

    public void LoadGame()
    {
        string temporalExistingFile = Application.persistentDataPath + "/" + _TemporalSavefileName;
        string existingFile = Application.persistentDataPath + "/" + _SavefileName;

        string jsonContent = "";

        if (File.Exists(temporalExistingFile)) 
            jsonContent = File.ReadAllText(temporalExistingFile);
        else if (File.Exists(existingFile))
            jsonContent = File.ReadAllText(existingFile);


        if (jsonContent != "")
        {
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

            for (int i = 0; i < ImportantSpawns.Count; i++) 
            {
                ImportantSpawns[i].Picked = info.ImportantSpawns[i].Picked;
                ImportantSpawns[i].Object = _Items.FromID(info.ImportantSpawns[i].ObjectId);
            }

            for (int i = 0; i < NormalSpawns.Count; i++)
            {
                NormalSpawns[i].Picked = info.NormalSpawns[i].Picked;
                NormalSpawns[i].Object = _Items.FromID(info.NormalSpawns[i].ObjectId);
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
            _Player.transform.position = info.Position;

            //Puzles
            _PuzzleManager.clockPuzzleCompleted = info.ClockPuzzle;
            _PuzzleManager.isHieroglyphicCompleted = info.HieroglyphicPuzzle;
            _PuzzleManager.bookPuzzleCompleted = info .BookPuzzle;
            _PuzzleManager.poemPuzzleCompleted = info.PoemPuzzle;
            _PuzzleManager.isMorseCompleted = info .MorsePuzzle;
            _PuzzleManager.weaponPuzzleCompleted = info.WeaponPuzzle;

            _PuzzleManager.LoadGame();

            //Enemics
            _Fat.position = info.FatEnemy;
            _Fast.position = info.FastEnemy;
            _Blind.position = info.BlindEnemy;

            //Puzle llibres
            bookCell.SetBook(_Items.FromID(info.CellBook.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook.bookGO).id));
            bookCell2.SetBook(_Items.FromID(info.CellBook2.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook2.bookGO).id));
            bookCell3.SetBook(_Items.FromID(info.CellBook3.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook3.bookGO).id));
            bookCell4.SetBook(_Items.FromID(info.CellBook4.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook4.bookGO).id));
            bookCell5.SetBook(_Items.FromID(info.CellBook5.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook5.bookGO).id));
            bookCell6.SetBook(_Items.FromID(info.CellBook6.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook6.bookGO).id));
            bookCell7.SetBook(_Items.FromID(info.CellBook7.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook7.bookGO).id));
            bookCell8.SetBook(_Items.FromID(info.CellBook8.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook8.bookGO).id));
            bookCell9.SetBook(_Items.FromID(info.CellBook9.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook9.bookGO).id));
            bookCell10.SetBook(_Items.FromID(info.CellBook10.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook10.bookGO).id));
            bookCell11.SetBook(_Items.FromID(info.CellBook11.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook11.bookGO).id));
            bookCell12.SetBook(_Items.FromID(info.CellBook12.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook12.bookGO).id));
            bookCell13.SetBook(_Items.FromID(info.CellBook13.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook13.bookGO).id));
            bookCell14.SetBook(_Items.FromID(info.CellBook14.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook14.bookGO).id));
            bookCell15.SetBook(_Items.FromID(info.CellBook15.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook15.bookGO).id));
            bookCell16.SetBook(_Items.FromID(info.CellBook16.bookGO).GetComponent<Book>(), _Books.FromID(_Items.FromID(info.CellBook16.bookGO).id));

            if (File.Exists(temporalExistingFile))
                File.Delete(temporalExistingFile);
        }

        GameManager.instance.onLoadedScene -= LoadGame;
    }

    public void LoadConfig()
    {
        string existingFile = Application.persistentDataPath + "/" + _SavefileNameConfig;

        if (File.Exists(existingFile))
        {
            string jsonContent = File.ReadAllText(existingFile);
            SaveConfig info = JsonUtility.FromJson<SaveConfig>(jsonContent);

            _Settings.currentVolumeValue = info.MusicValue;
            _Settings.currentSfxValue = info.SFXValue;
            _Settings.currentFpsValue = info.FPSValue;
            _Settings.currentFOVValue = info.FOVValue;
            _Settings.currentVSyncState = info.VSync;
        }

        _Settings.isInitialScene = true;
        _Settings.StartOptions();

        GameManager.instance.onLoadedScene -= LoadConfig;
        GameManager.instance.onNewScene -= LoadConfig;
    }
}
