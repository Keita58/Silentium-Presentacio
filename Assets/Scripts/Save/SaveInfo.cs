using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static InventorySO;

[Serializable]
public class SaveInfo
{
    //Inventari
    public List<ItemSlotSave> Inventory;
    public List<ItemSlotSave> ChestInventory;
    public int[] NotesInventory;

    //Jugador
    public int Hp;
    public bool Silencer;
    public int SilencerUses;
    public int Ammo;

    //Puzles
    public bool ClockPuzzle;
    public bool HieroglyphicPuzzle;
    public bool BookPuzzle;
    public bool PoemPuzzle;
    public bool MorsePuzzle;
    public bool WeaponPuzzle;

    //Configuracio



    //Valors post-render


}
