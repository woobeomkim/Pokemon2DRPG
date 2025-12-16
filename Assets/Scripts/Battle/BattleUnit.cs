using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Net.NetworkInformation;
using Unity.VisualScripting;

public class BattleUnit : MonoBehaviour
{
    [SerializeField] bool isPlayerUnit;
    [SerializeField] BattleHud hud;
    public bool IsPlayerUnit => isPlayerUnit;

    public BattleHud Hud => hud;
    public Pokemon Pokemon { get; set; }

    Image image;
    Vector3 originalPos;
    Color originalColor;

    private void Awake()
    {
        image = GetComponent<Image>();
        originalPos = image.transform.localPosition;
        originalColor = image.color;
    }

    public void Setup(Pokemon pokemon)
    {
        Pokemon = pokemon;

        if (isPlayerUnit)
            image.sprite = Pokemon.Base.BackSprite;
        else
            image.sprite = Pokemon.Base.FrontSprite;

        hud.gameObject.SetActive(true);

        hud.SetData(pokemon);

        transform.localScale = new Vector3(1, 1, 1);
        image.transform.localPosition = originalPos;
        image.color = originalColor;

        PlayEnterAnimation();
    }

    public void Clear()
    {
        hud.gameObject.SetActive(false);
    }

    public void SetSelected(bool selected)
    {
        image.color = (selected) ? GlobalSettings.i.HighlightedColor : originalColor;
    }

    public void PlayEnterAnimation()
    {
        if (isPlayerUnit)
            image.transform.localPosition = new Vector3(-500f, originalPos.y);
        else
            image.transform.localPosition = new Vector3(500f, originalPos.y);

        transform.DOLocalMoveX(originalPos.x, 1.0f);
    }

    public void PlayAttackAnimation()
    {
        var sequence = DOTween.Sequence();
        if (isPlayerUnit)
            sequence.Append(transform.DOLocalMoveX(originalPos.x + 50f, 0.25f));
        else
            sequence.Append(transform.DOLocalMoveX(originalPos.x - 50f, 0.25f));

        sequence.Append(transform.DOLocalMoveX(originalPos.x, 0.25f));

    }

    public void PlayHitAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOColor(Color.gray, 0.1f));
        sequence.Append(image.DOColor(originalColor, 0.1f));
    }

    public void PlayFaintAniamtion()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.transform.DOLocalMoveY(originalPos.y - 150f, 0.5f));
        sequence.Join(image.DOFade(0.0f, 0.5f));
    }

    public IEnumerator PlayCaptureAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(0, 0.5f));
        sequence.Join(image.transform.DOLocalMoveY(originalPos.y + 50f, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(0.3f,0.3f,1f),0.5f));
        yield return sequence.WaitForCompletion();
    }

    public IEnumerator PlayBreakOutAnimation()
    {
        var sequence = DOTween.Sequence();
        sequence.Append(image.DOFade(1, 0.5f));
        sequence.Join(image.transform.DOLocalMoveY(originalPos.y, 0.5f));
        sequence.Join(image.transform.DOScale(new Vector3(1f, 1f, 1f), 0.5f));
        yield return sequence.WaitForCompletion();
    }
}
