using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wallet : MonoBehaviour
{
    [SerializeField] float money;


    public event Action onMoneyChanged;
    public static Wallet i { get; private set; }

    private void Awake()
    {
        i = this;
    }
    public void AddMoney(float amount)
    {
        money += amount;
        onMoneyChanged?.Invoke();
    }

    public void TakeMoney(float amount)
    {
        money -= amount;
        onMoneyChanged?.Invoke();
    }

    public float Money => money;
}
