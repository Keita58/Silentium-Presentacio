using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class Item : ScriptableObject
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Sprite Sprite { get; }

    public abstract GameObject prefabToEquip { get; }

    public abstract GameObject prefabToInstantiate { get; }

    public abstract List<Item> combinableItems { get; }

    public abstract ItemTypes ItemType { get; }

    public abstract bool isStackable { get; }

    public abstract bool isUsable { get; }

    public abstract bool isEquipable { get; }

    public abstract bool isCombinable { get; }

    public abstract int id { get; }
    
    public abstract void Use();

    public abstract void Combine(Item item);

    public abstract void Equip();
}

public enum ItemTypes
{
    AMOXICILINE, 
    PARACETAMOL,
    TAPE, 
    BOTTLE,
    AMMO, 
    KEY,
    GUN,
    BOOK1,
    BOOK2,
    BOOK3,
    BOOK4,
    BOOK5,
    PLASTICBOTTLE,
    SILENCER,
    SAVEIMAGE
}
