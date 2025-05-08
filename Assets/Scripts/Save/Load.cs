using System.IO;
using UnityEngine;
using System.Collections.Generic;
using static InventorySO;

public class Load : MonoBehaviour
{
    private const string _SavefileName = "silentium_savegame.json";

    [SerializeField] private InventorySO _Inventory;
    [SerializeField] private InventorySO _ChestInventory;
    [SerializeField] private NoteInventorySO _NotesInventory;
    [SerializeField] private BDNotes _Notes;
    [SerializeField] private BDItems _Items;

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


            //Carregar el mapa
            //SceneManager.LoadScene("Map");
        }
        else
        {
            //Posar l'avís de que no hi ha un joc guardat
        }
    }
}
