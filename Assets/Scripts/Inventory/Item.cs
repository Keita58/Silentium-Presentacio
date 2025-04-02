using System;
using UnityEngine;

public abstract class Item : ScriptableObject
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Sprite Sprite { get; }

    public abstract GameObject prefabToEquip {  get; }

    public abstract void Use();

    public abstract void Combine();

    public abstract void Equip();
}
