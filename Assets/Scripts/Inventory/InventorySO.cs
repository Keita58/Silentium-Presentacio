using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "InventorySO", menuName = "Scriptable Objects/Items/InventorySO")]
public class InventorySO : ScriptableObject
{
    [SerializeField]
    public List<ItemSlot> items;

    private void Awake()
    {
        items = new List<ItemSlot>();
    }

    [Serializable]
    public class ItemSlot
    {
        [SerializeField]
        public Item item;
        [SerializeField]
        public int amount;

        public ItemSlot(Item obj)
        {
            item = obj;
            amount = 1;
        }
    }

    public void UseItem(Item usedItem)
    {
        ItemSlot item = GetItem(usedItem);
        if (item == null)
            return;

        item.amount--;
        if (item.amount<= 0)
            items.Remove(item);

    }

    public void AddItem(Item usedItem)
    {
        ItemSlot item = GetItem(usedItem);
        if (item == null) items.Add(new ItemSlot(usedItem));
        else
            item.amount++;

    }

    private ItemSlot GetItem(Item item)
    {
        foreach (ItemSlot slot in items)
        {
            if (slot.item == item)
            {
                return slot;
            }

        }
        return null;
    }
}
