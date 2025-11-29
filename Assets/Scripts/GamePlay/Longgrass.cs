using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Longgrass : MonoBehaviour, IPlayerTriggerable
{

    public void OnPlayerTriggered(PlayerController player)
    {
        if (UnityEngine.Random.Range(1, 101) <= 10)
        {
            player.Character.Animator.IsMoving = false;
            GameController.i.StartBattle();
        }
    }
    public bool TriggerRepeatedly => true;
}
