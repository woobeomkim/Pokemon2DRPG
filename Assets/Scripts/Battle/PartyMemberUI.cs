using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMemberUI : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text lvlText;
    [SerializeField] HPBar hpBar;

    Pokemon pokemon;

    public void Init(Pokemon pokemon)
    {
        this.pokemon = pokemon;
        UpdateData();

        this.pokemon.OnHpChagnged += UpdateData;
    }

    void UpdateData()
    {
        nameText.text = pokemon.Base.Name;
        lvlText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHP);
    }

    public void SetSelected(bool selected)
    {
        if (selected)
            nameText.color = GlobalSettings.i.HighlightedColor;
        else
            nameText.color = Color.black;
    }
}
