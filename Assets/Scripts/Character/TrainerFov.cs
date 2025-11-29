using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainerFov : MonoBehaviour, IPlayerTriggerable
{

    public void OnPlayerTriggered(PlayerController player)
    {
        player.Character.Animator.IsMoving = false;
        var trainer = GetComponentInParent<TrainerController>();
        if(trainer != null)
            GameController.i.OnEnterTrainersView(trainer);
    }
    public bool TriggerRepeatedly => false;

}
