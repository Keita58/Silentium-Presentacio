using System;
using System.Collections.Generic;
using UnityEngine;
using static InventorySO;
using static PickObject;
using static CellBook;

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
    public Vector3 Position;

    //Puzles
    public bool ClockPuzzle;
    public bool HieroglyphicPuzzle;
    public bool BookPuzzle;
    public bool PoemPuzzle;
    public bool MorsePuzzle;
    public bool WeaponPuzzle;

    //Puzle llibres
    public List<CellBookSave> CellBooks;

    //Valors post-render
    //Chromatic
    public bool ChromaticAberration;
    public float IntensityChromaticAberration;
    public int Samples;

    //Film Grain
    public bool FilmGrain;
    public float IntensityFilmGrain;

    //Fog
    public bool Fog;

    //Enemics
    public Vector3 BlindEnemy;
    public Vector3 FastEnemy;
    public Vector3 FatEnemy;

    //Spawn items
    public List<PickObjectSave> ImportantSpawns;
    public List<PickObjectSave> NormalSpawns;
}
