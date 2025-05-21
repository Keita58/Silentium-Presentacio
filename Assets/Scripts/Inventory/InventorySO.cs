using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
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
        [SerializeField]
        public bool stackable;

        public ItemSlot(Item obj, bool stackable)
        {
            item = obj;
            amount = 1;
            this.stackable = stackable;
        }


        public ItemSlot(Item item, int amount, bool stackable) : this(item, item.isStackable)
        {
            this.amount = amount;
            this.stackable = stackable;
        }
    }

    [Serializable]
    public class ItemSlotSave
    {
        [SerializeField] public int itemId;

        [SerializeField] public int amount;

        [SerializeField] public bool stackable;

        public ItemSlotSave(int itemId, int amount, bool stackable)
        {
            this.itemId = itemId;
            this.amount = amount;
            this.stackable = stackable;
        }
    }

    public void UseItem(Item usedItem)
    {
        ItemSlot item = GetItem(usedItem);
        if (item == null)
            return;
        if (usedItem is AmmunitionItem)
            item.amount -= 6;
        else
            item.amount--;
        if (item.amount<= 0)
            items.Remove(item);

    }

    public void AddItem(Item usedItem)
    {
        ItemSlot item = GetItem(usedItem);
        if (item == null)
        {
            if (usedItem is AmmunitionItem)
                items.Add(new ItemSlot(usedItem, 6, usedItem.isStackable));
            else
                items.Add(new ItemSlot(usedItem, usedItem.isStackable));
        }
        else if (!usedItem.isStackable)
        {
            items.Add(new ItemSlot(usedItem, usedItem.isStackable));
        }
        else
        {
            if (usedItem is AmmunitionItem) item.amount += 6;
            else
                item.amount++;
        }

    }

    public ItemSlot GetItem(Item item)
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
