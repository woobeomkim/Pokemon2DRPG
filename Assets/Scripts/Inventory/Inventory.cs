using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.Dependencies.NCalc;
using UnityEngine;

public enum ItemCategory { Items,Pokeballs,Tms}

public class Inventory : MonoBehaviour
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
                RemoveItem(item, selectedCategory);
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

    public void RemoveItem(ItemBase item,int selectedCategory)
    {
        var currentSlot = GetSlotsByCategory(selectedCategory);
        var itemSlot = currentSlot.First(slot => slot.Item == item);
        itemSlot.Count--;
        
        if(itemSlot.Count == 0)
        {
            currentSlot.Remove(itemSlot);
        }

        onUpdated?.Invoke();
    }

    public ItemCategory GetCategoryFromItem(ItemBase item)
    {
        if (item is RecoveryItem)
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
}

[System.Serializable]
public class ItemSlot
{
    [SerializeField] ItemBase item;
    [SerializeField] int count;

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