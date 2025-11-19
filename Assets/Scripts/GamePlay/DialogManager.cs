using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogManager : MonoBehaviour
{
    [SerializeField] GameObject dialogBox;
    [SerializeField] Text dialogText;
    [SerializeField] int lettersPerSeconds;

    public static DialogManager i { get; set; }

    public event Action OnShowDialog;
    public event Action OnCloseDialog;

    Dialog dialog;
    Action onDialogFinished;

    private void Awake()
    {
        i = this;
    }
    
    public bool IsShowing { get; set; }

    public IEnumerator ShowDialog(Dialog dialog, Action onFinished = null)
    {
        yield return new WaitForEndOfFrame();

        OnShowDialog?.Invoke();
        
        IsShowing = true;
        this.dialog = dialog;
        onDialogFinished = onFinished;

        dialogBox.SetActive(true);
        StartCoroutine(TypeDialog(dialog.Lines[0]));
    }

    int currentLine = 0;
    bool isType;

    public void HandleUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Z) && !isType)
        {
            currentLine++;
            if (currentLine < dialog.Lines.Count)
            {
                StartCoroutine(TypeDialog(dialog.Lines[currentLine]));
            }
            else
            {
                currentLine = 0;
                IsShowing = false;
                dialogBox.SetActive(false);
                onDialogFinished?.Invoke();
                OnCloseDialog?.Invoke();
            }
        }
    }

    public IEnumerator TypeDialog(string dialog)
    {
        isType = true;
        dialogText.text = "";
        foreach (var letter in dialog.ToCharArray())
        {
            dialogText.text += letter;
            yield return new WaitForSeconds(1f / lettersPerSeconds);
        }
        isType = false;
    }
}
