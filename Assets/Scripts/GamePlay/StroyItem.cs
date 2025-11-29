using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StroyItem : MonoBehaviour, IPlayerTriggerable
{
    [SerializeField] Dialog dialog;


    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
       StartCoroutine(DialogManager.i.ShowDialog(dialog));
    }

    public bool TriggerRepeatedly => false;
}
