using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Sprite Sprite { get; }

    public abstract GameObject prefabToEquip {  get; }

    public abstract List<Item> combinableItems { get; }

    public abstract void Use();

    public abstract void Combine(Item item);

    public abstract void Equip();
}
