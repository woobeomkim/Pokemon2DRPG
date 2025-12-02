using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public enum ItemCategory { Items,Pokeballs,Tms}

public class Inventory : MonoBehaviour,ISavable
{
    [SerializeField] List<ItemSlot> slots;
    [SerializeField] List<ItemSlot> pokeballSlots;
    [SerializeField] List<ItemSlot> tmSlots;

    List<List<ItemSlot>> allSlots;

    public event Action onUpdated;

    private void Awake()
    {
        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots };
    }

    public static List<string> ItemCategories = new List<string>()
    {
        "ITEMS", "POKEBALLS","Tms & Hms"
    };

    public List<ItemSlot> GetSlotsByCategory(int categoryIndex)
    {
        return allSlots[categoryIndex];
    }

    public ItemBase GetItem(int itemIndex, int categoryIndex)
    {
        var currentSlot = GetSlotsByCategory(categoryIndex);
        return currentSlot[itemIndex].Item;
    }

    public ItemBase UseItem(int itemIndex, Pokemon selectedPokemon, int selectedCategory)
    {
        var item = GetItem(itemIndex, selectedCategory);
        bool itemUsed = item.Use(selectedPokemon);

        if(itemUsed)
        {
            if(!item.IsReusable)
                RemoveItem(item);
            return item;
        }

        return null;
    }

    public void AddItem(ItemBase item, int count = 1)
    {
        int categroy = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(categroy);

        var itemSlot = currentSlot.FirstOrDefault(slot => slot.Item == item);
    
        if(itemSlot != null)
        {
            itemSlot.Count += count;
        }
        else
        {
            currentSlot.Add(new ItemSlot()
            {
                Item = item,
                Count = count
            });
        }

        onUpdated?.Invoke();
    }

    public void RemoveItem(ItemBase item)
    {
        int selectedCategory = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count--;
        
        if(itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
        }

        onUpdated?.Invoke();
    }

    public bool HasItem(ItemBase item)
    {
        int selectedCategory = (int)GetCategoryFromItem(item);
        var currentSlot = GetSlotsByCategory(selectedCategory);

        return currentSlot.Exists(slot => slot.Item == item);
    }

    public ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem || item is EvolutionItem)
            return ItemCategory.Items;
        else if (item is PokeballItem)
            return ItemCategory.Pokeballs;
        else
            return ItemCategory.Tms;
    }

    public static Inventory GetInventory()
    {
        return FindObjectOfType<PlayerController>().GetComponent<Inventory>();
    }

    public object CaptureState()
    {
        var saveData = new InventorySaveData()
        {
            items = slots.Select(s => s.GetSaveData()).ToList(),
            pokeballs = pokeballSlots.Select(p => p.GetSaveData()).ToList(),
            tms = tmSlots.Select(t => t.GetSaveData()).ToList()
        };

        return saveData;
    }

    public void RestoreState(object state)
    {
        var saveData = (InventorySaveData)state;

        slots = saveData.items.Select(i => new ItemSlot(i)).ToList();
        pokeballSlots = saveData.pokeballs.Select(s => new ItemSlot(s)).ToList();
        tmSlots = saveData.tms.Select(t => new ItemSlot(t)).ToList();

        allSlots = new List<List<ItemSlot>> { slots, pokeballSlots, tmSlots };
        onUpdated?.Invoke();
    }
}

[System.Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

    public ItemSlot()
    {

    }

    public ItemSlot(ItemSaveData saveData)
    {
        item = ItemDB.GetObjectByName(saveData.name);
        count = saveData.count;
    }

    public ItemSaveData GetSaveData()
    {
        var saveData = new ItemSaveData()
        {
            name = item.name,
            count = count
        };

        return saveData;
    }

    public ItemBase Item
    {
        get => item;
        set => item = value;
    }
    public int Count
    {
        get => count;
        set => count = value;
    }
}

[System.Serializable]
public class ItemSaveData
{
    public string name;
    public int count;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items;
    public List<ItemSaveData> pokeballs;
    public List<ItemSaveData> tms;
}