using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleDialog : MonoBehaviour
{
    [SerializeField] Color highlightedColor;
    [SerializeField] int lettersPerSeconds;
    [SerializeField] Text dialogText;

    [SerializeField] GameObject actionSelector;
    [SerializeField] GameObject moveSelector;
    [SerializeField] GameObject moveDetails;

    [SerializeField] List<Text> actionTexts;
    [SerializeField] List<Text> moveTexts;

    [SerializeField] Text ppText;
    [SerializeField] Text typeText;

    public List<Text> ActionTexts => actionTexts;
    public List<Text> MoveTexts => moveTexts;

    public void SetDialog(string dialog)
    {
        dialogText.text = dialog;
    }

    public IEnumerator TypeDialog(string dialog)
    {
        dialogText.text = "";

        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSeconds);
        }

        yield return new WaitForSeconds(1.0f);
    }

    public void EnabledDialogText(bool enabled)
    {
        dialogText.enabled = enabled;
    }

    public void EnabledActionSelector(bool enabled)
    {
        actionSelector.gameObject.SetActive(enabled);
    }

    public void EnabledMoveSelector(bool enabled)
    {
        moveSelector.gameObject.SetActive(enabled);
        moveDetails.gameObject.SetActive(enabled);
    }

    public void SetMoveNames(List<Move> moves)
    {
        for (int i = 0; i < moveTexts.Count; i++) 
        {
            if (i < moves.Count)
                moveTexts[i].text = moves[i].Base.Name;
            else
                moveTexts[i].text = "-";
        }
    }

    public void UpdateActionSelection(int selected)
    {
        for (int i = 0; i < actionTexts.Count; i++) 
        {
            if (i == selected)
                actionTexts[i].color = highlightedColor;
            else
                actionTexts[i].color = Color.black;
        }
    }

    public void UpdateMoveSelection(int selected, Move move)
    {
        for (int i = 0; i < moveTexts.Count; i++)
        {
            if (i == selected)
                moveTexts[i].color = highlightedColor;
            else
                moveTexts[i].color = Color.black;

        }
        ppText.text = $"PP{move.PP}/{move.Base.PP}";
        typeText.text = move.Base.Type.ToString();
    }
}
