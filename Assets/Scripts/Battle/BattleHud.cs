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
        if(this.pokemon != null)
        {
            this.pokemon.OnStatusChanged -= SetStatusText;
            this.pokemon.OnHpChagnged -= UpdateHP;
        }

        this.pokemon = pokemon;
        nameText.text = pokemon.Base.Name;
        hpBar.SetHP((float)pokemon.HP / pokemon.MaxHP);
        SetLevel();
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
        this.pokemon.OnStatusChanged += SetStatusText;
        this.pokemon.OnHpChagnged += UpdateHP;
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

    public void SetLevel()
    {
        lvlText.text = "Lvl " + pokemon.Level;
    }

    public IEnumerator SetExpSmooth(bool reset = false)
    {
        if (expBar == null) yield break;

        if (reset)
            expBar.transform.localScale = new Vector3(0, 1, 1);
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

    void UpdateHP()
    {
        StartCoroutine(UpdateHPAsync());
    }

    public IEnumerator UpdateHPAsync()
    {
        yield return hpBar.SetHPSmooth((float)pokemon.HP / pokemon.MaxHP);
    }

    public IEnumerator WaitForHPUpdate()
    {
        yield return new WaitUntil(() => hpBar.IsUpdating == false);
    }
}
