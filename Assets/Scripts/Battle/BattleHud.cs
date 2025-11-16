using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleHud : MonoBehaviour
{
    [SerializeField] Text nameText;
    [SerializeField] Text lvlText;
    [SerializeField] Text statusText;
    [SerializeField] HPBar hpBar;

    [SerializeField] Color psnColor;
    [SerializeField] Color brnColor;
    [SerializeField] Color parColor;
    [SerializeField] Color frzColor;
    [SerializeField] Color slpColor;

    public Dictionary<ConditionID, Color> statusColor;

    Pokemon pokemon;

    public void SetData(Pokemon pokemon)
    {
        this.pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        lvlText.text = "Lvl " + pokemon.Level;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHP);

        statusColor = new Dictionary<ConditionID, Color>()
        {
            {ConditionID.psn,psnColor },
            {ConditionID.brn,brnColor },
            {ConditionID.par,parColor },
            {ConditionID.frz,frzColor },
            {ConditionID.slp,slpColor },
        };

        SetStatusText();
        pokemon.OnStatusChanged += SetStatusText;
    }

    public void SetStatusText()
    {
        if(pokemon.Status == null)
        {
            statusText.text = "";
        }
        else
        {
            statusText.text = pokemon.Status.ID.ToString().ToUpper();
            statusText.color = statusColor[pokemon.Status.ID];
        }
    }

    public IEnumerator UpdateHP()
    {
        if (pokemon.HPChanged)
        {
            yield return hpBar.SetHPSmooth((float)pokemon.HP / pokemon.MaxHP);
            pokemon.HPChanged = false;
        }
    }
}
