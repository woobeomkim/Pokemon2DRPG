using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HPBar : MonoBehaviour
{
    [SerializeField] GameObject health;

    public bool IsUpdating { get; private set; }
    public void SetHP(float hpNormalized)
    {
        health.transform.localScale = new Vector3(hpNormalized, 1.0f, 1.0f);
    }

    public IEnumerator SetHPSmooth(float newHP)
    {
        IsUpdating = true;
        float curHP = health.transform.localScale.x;
        float changeAmt = curHP - newHP;

        while ((curHP - newHP) > Mathf.Epsilon) 
        {
            curHP = curHP - changeAmt * Time.deltaTime;
            health.transform.localScale = new Vector3(curHP, 1.0f, 1.0f);
            yield return null;
        }
        health.transform.localScale = new Vector3(newHP, 1.0f, 1.0f);
        IsUpdating = false;
    }
}
