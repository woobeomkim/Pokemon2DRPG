using DG.Tweening;
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
    [SerializeField] GameObject expBar;

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
        SetExp();

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

    public void SetExp()
    {
        if (expBar == null) return;

        float normalizedExp = GetNormalizedExp();
        expBar.transform.localScale = new Vector3(normalizedExp, 1, 1);
    }

    public IEnumerator SetExpSmooth()
    {
        if (expBar == null) yield break;

        float normalizedExp = GetNormalizedExp();
        yield return expBar.transform.DOScaleX(normalizedExp, 1.5f).WaitForCompletion();
    }


    float GetNormalizedExp()
    {
        int currLevelExp = pokemon.Base.GetExpForLevel(pokemon.Level);
        int nextLevelExp = pokemon.Base.GetExpForLevel(pokemon.Level + 1);

        float normalizedExp = (float)(pokemon.Exp - currLevelExp) / (nextLevelExp - currLevelExp);
        return Mathf.Clamp01(normalizedExp);
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
